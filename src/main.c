#include "os.h"
#include "cx.h"

#include "os_io_seproxyhal.h"
#include "string.h"
#include "glyphs.h"

#include "ux_common.h"
#if defined (TARGET_BLUE)
  #include "ux_blue.h"
#elif defined (TARGET_NANOS)
  #include "ux_nanos.h"
#elif defined (TARGET_ARAMIS)
#else
#error unknown TARGET_ID
#endif

unsigned char G_io_seproxyhal_spi_buffer[IO_SEPROXYHAL_BUFFER_SIZE_B];

unsigned char string_buffer[64];

unsigned int demo_counter;
ux_state_t ux;



#define DERIVE_PATH         "72'/69/76/76/79" //HELLO
#define DERIVE_PATH_LEN     (sizeof(DERIVE_PATH)-1)
#define DEVICE_KEY_STR      "Device"
#define DEVICE_KEY_STR_LEN  (sizeof(DEVICE_KEY_STR)-1)
#define AUTH_KEY_STR      "Auth"
#define AUTH_KEY_STR_LEN  (sizeof(AUTH_KEY_STR)-1)
#define DEVICE_GUID_STR     "ID"
#define DEVICE_GUID_STR_LEN (sizeof(DEVICE_GUID_STR)-1)

unsigned char secret_computed;
unsigned char derived_key[32];

unsigned char device_id[32];
unsigned char nonce_srv[32];
uint8_t refreshUi;
uint8_t replySize;

const bagl_element_t* ui_idle_menu_preprocessor(const ux_menu_entry_t* entry, bagl_element_t* element);

void compute_device_secrets(void) {
  if (!secret_computed) {
    os_perso_derive_node_bip32(CX_CURVE_SECP256K1, DERIVE_PATH, DERIVE_PATH_LEN, derived_key, NULL);
    cx_sha256_t context;
    // get device_key from hash(DEVICE_KEY_STR,derived_key)
    cx_sha256_init(&context);
    cx_hash(&context,0,DEVICE_KEY_STR,DEVICE_KEY_STR_LEN,NULL);
    cx_hash(&context,CX_LAST,derived_key,32,device_key);
    // get auth_key from hash(AUTH_KEY_STR,derived_key)
    cx_sha256_init(&context);
    cx_hash(&context,0,AUTH_KEY_STR,AUTH_KEY_STR_LEN,NULL);
    cx_hash(&context,CX_LAST,derived_key,32,auth_key);
    // get device_id from hash(DEVICE_GUID_STR,derived_key)
    cx_sha256_init(&context);
    cx_hash(&context,0,DEVICE_GUID_STR,DEVICE_GUID_STR_LEN,NULL);
    cx_hash(&context,CX_LAST,derived_key,32,device_id);
  }
  secret_computed=1;
}

unsigned int compute_login_reply(void) {    
  cx_hmac_sha256_t hmac_context;
  unsigned char device_hmac[32];
  unsigned int rx = 0;

  // confirm
  // Command APDU:
  // ------------- 
  // header (5)
  // HMACsrv (32)
  // NonceSk (32)
  // NonceDk (32)
  //
  // Response APDU:
  // --------------
  // HMACdk (32)
  // HMACsk (32)
  // 1/ Validate deviceHMAC = HMAC(auth_key, nonce_srv || NonceDk || NonceSk)
  cx_hmac_sha256_init(&hmac_context,auth_key,32);         
  cx_hmac(&hmac_context,0,      nonce_srv,32,NULL);
  cx_hmac(&hmac_context,0,      G_io_apdu_buffer+5+32+32,32,NULL);
  cx_hmac(&hmac_context,CX_LAST,G_io_apdu_buffer+5+32,32,device_hmac);
  os_xor(device_hmac, G_io_apdu_buffer+5, device_hmac, 32);
  for (rx=1; rx < 32; rx++) {
    device_hmac[rx] |= device_hmac[rx-1];
  }
  if (device_hmac[31] != 0 || rx != 32) {
    G_io_apdu_buffer[0] = SW_CONDITIONS_NOT_SATISFIED >> 8; // Add code to indicate that HMAC is wrong
    G_io_apdu_buffer[1] = SW_CONDITIONS_NOT_SATISFIED & 0xff;
    return 2;
  }
  // 2/ Compute HMACdk = HMAC(device_key, NonceDk)
  cx_hmac_sha256(device_key,32,G_io_apdu_buffer+5+32+32,32,G_io_apdu_buffer);
  // 3/ Compute HMACsk = HMAC(auth_key, HMACdk || NonceSk)   
  cx_hmac_sha256_init(&hmac_context,auth_key,32);         
  cx_hmac(&hmac_context,0,      G_io_apdu_buffer,32,NULL);
  cx_hmac(&hmac_context,CX_LAST,G_io_apdu_buffer+5+32,32,G_io_apdu_buffer+32);
  G_io_apdu_buffer[32+32] = SW_OK >> 8;
  G_io_apdu_buffer[32+32+1] = SW_OK & 0xff;
  return 32+32+2;
}

unsigned short io_exchange_al(unsigned char channel, unsigned short tx_len) {

  switch(channel&~(IO_FLAGS)) {

  case CHANNEL_KEYBOARD:
    break;

  // multiplexed io exchange over a SPI channel and TLV encapsulated protocol
  case CHANNEL_SPI:
    if (tx_len) {
      io_seproxyhal_spi_send(G_io_apdu_buffer, tx_len);

      if (channel & IO_RESET_AFTER_REPLIED) {
        reset();
      }
      return 0; // nothing received from the master so far (it's a tx transaction)
    }
    else {
      return io_seproxyhal_spi_recv(G_io_apdu_buffer, sizeof(G_io_apdu_buffer), 0);
    }

  default:
    THROW(INVALID_PARAMETER);
  }
  return 0;
}

void sample_main(void) {
  volatile unsigned int rx = 0;
  volatile unsigned int tx = 0;
  volatile unsigned int flags = 0;

  // DESIGN NOTE: the bootloader ignores the way APDU are fetched. The only goal is to retrieve APDU.
  // When APDU are to be fetched from multiple IOs, like NFC+USB+BLE, make sure the io_event is called with a 
  // switch event, before the apdu is replied to the bootloader. This avoid APDU injection faults.
  for (;;) {
    volatile unsigned short sw = 0;

    BEGIN_TRY {
      TRY {
        rx = tx;
        tx = 0; // ensure no race in catch_other if io_exchange throws an error
        rx = io_exchange(CHANNEL_APDU|flags, rx);
        flags = 0;

        // no apdu received, well, reset the session, and reset the bootloader configuration
        if (rx == 0) {
          THROW(0x6982);
        }

        if (G_io_apdu_buffer[0] != 0x80) {
          THROW(0x6E00);
        }

        // unauthenticated instruction
        switch (G_io_apdu_buffer[1]) {
          // get challenge
          case 0x84:
            cx_rng(nonce_srv,32);
            memcpy(G_io_apdu_buffer,nonce_srv,32);
            tx = 32;
            THROW(SW_OK);

          // mutual authenticate
          case 0x82:
            compute_device_secrets();
            if (!N_storage.dont_confirm_login) {
              tx = compute_login_reply();
              // status word is appended during compute
              //THROW(SW_OK);
            }
            else {
              flags |= IO_ASYNCH_REPLY;
              ui_confirm_login_init();
              //UX_DISPLAY(ui_confirm_login_nanos,NULL);
            }
            break;

          case 0xCA: 
            switch (G_io_apdu_buffer[2]) {
              case 0x00: // device_id
                compute_device_secrets();
                memcpy(G_io_apdu_buffer,device_id,16);
                tx = 16;
                THROW(SW_OK);
                break;
              case 0x01:
                // always asks for a user content
                compute_device_secrets();
                flags |= IO_ASYNCH_REPLY;
                ui_confirm_registration_init();
                //UX_DISPLAY(ui_confirm_registration_nanos,NULL);
                break;
              case 0x02: //D-Lock state
                G_io_apdu_buffer[0] = N_storage.dynamic_lock;
                tx = 1;
                THROW(SW_OK);
                break;
              default:
                THROW(0x6B00);
                break;
            }
            // temp for testing
            //tx = 32;

            // Protect by a user consent
            // TODO show user consent
            // UX_DISPLAY(...);
            // flags = IO_FLAGS_REPLY_ASYNCH;
            break;

          case 0xFF: // return to dashboard
            goto return_to_dashboard;

          default:
            THROW(0x6D00);
            break;
        }
      }
      CATCH_OTHER(e) {
        switch(e & 0xF000) {
          case 0x6000:
          case SW_OK:
            sw = e;
            break;
          default:
            sw = 0x6800 | (e&0x7FF);
            break;
        }
        // Unexpected exception => report 
        G_io_apdu_buffer[tx] = sw>>8;
        G_io_apdu_buffer[tx+1] = sw;
        tx += 2;
      }
      FINALLY {
        
      }
    }
    END_TRY;
  }

return_to_dashboard:
  return;
}

void io_seproxyhal_display(const bagl_element_t * element) {
  return io_seproxyhal_display_default(element);
}


unsigned char io_event(unsigned char channel) {

  // can't have more than one tag in the reply, not supported yet.
  switch (G_io_seproxyhal_spi_buffer[0]) {

    case SEPROXYHAL_TAG_FINGER_EVENT:
    		UX_FINGER_EVENT(G_io_seproxyhal_spi_buffer);
    		break;


    // power off if long push, else pass to the application callback if any
    case SEPROXYHAL_TAG_BUTTON_PUSH_EVENT:
        UX_BUTTON_PUSH_EVENT(G_io_seproxyhal_spi_buffer);
        break;


    default:
        UX_DEFAULT_EVENT();
        break;

    case SEPROXYHAL_TAG_DISPLAY_PROCESSED_EVENT:

      UX_DISPLAYED_EVENT({});
      //UX_DISPLAYED_EVENT(display_done(););
      break;
    case SEPROXYHAL_TAG_TICKER_EVENT:

        icon_change_timer_cnt++;
        UX_TICKER_EVENT(G_io_seproxyhal_spi_buffer, 
        {//// here
		  // only allow display when not locked of overlayed by an OS UX.
          if (UX_ALLOWED && ux_step_count) {
            // prepare next screen
            ux_step = (ux_step+1)%ux_step_count;
            // redisplay screen
            UX_REDISPLAY(); 
          }
        });
        break;
  }
  // close the event if not done previously (by a display or whatever)
  if (!io_seproxyhal_spi_is_status_sent()) {
    io_seproxyhal_general_status();
  }
  
  // command has been processed, DO NOT reset the current APDU transport
  return 1;
}

void app_exit(void) {
    BEGIN_TRY_L(exit) {
        TRY_L(exit) {
            os_sched_exit(-1);
        }
        FINALLY_L(exit) {

        }
    }
    END_TRY_L(exit);
}

__attribute__ ((section(".boot")))
int main(void) {
  
  // exit critical section
  __asm volatile ("cpsie i");

  secret_computed = 0;
  #if defined (TARGET_NANOS)
    icon_hack_flag = 0;
  #endif
  
  // ensure exception will work as planned
  os_boot();

  UX_INIT();

  BEGIN_TRY {
    TRY {
      io_seproxyhal_init();

      // HID // USB_power(1);
      USB_CCID_power(1);

	    ui_idle_init();

      sample_main();
    }
    CATCH_ALL {
      
    }
    FINALLY {

    }
  }
  END_TRY;

  app_exit();
  return 0;
}
