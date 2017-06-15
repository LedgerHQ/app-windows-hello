#include "ux_common.h"

unsigned int ux_step;
unsigned int ux_step_count;
unsigned char device_key[32];
unsigned char auth_key[32];


/*typedef struct internalStorage_t {
// #define STORAGE_MAGIC 0xDEAD1337
//     uint32_t magic;
    uint32_t dont_confirm_login;
    uint32_t dynamic_lock;
} internalStorage_t;*/

WIDE internalStorage_t N_storage_real;
#define N_storage (*(WIDE internalStorage_t *)PIC(&N_storage_real)) 

void hello_register_cancel(void){
	G_io_apdu_buffer[0] = LOGIN_DENIED_BY_USER >> 8;
    G_io_apdu_buffer[1] = LOGIN_DENIED_BY_USER & 0xff;
    io_exchange(CHANNEL_APDU | IO_RETURN_AFTER_TX, 2);
    ui_idle_init();
} 

void hello_register_confirm(void){
	uint8_t replySize;
	memcpy(G_io_apdu_buffer,device_key,32);
    memcpy(G_io_apdu_buffer+32,auth_key,32);
    G_io_apdu_buffer[32+32] = SW_OK >> 8;
    G_io_apdu_buffer[32+32+1] = SW_OK & 0xff;
    replySize = 32+32+2;
    io_exchange(CHANNEL_APDU | IO_RETURN_AFTER_TX, replySize);
    ui_idle_init();
}

void hello_login_cancel(void){
	G_io_apdu_buffer[0] = LOGIN_DENIED_BY_USER >> 8;
    G_io_apdu_buffer[1] = LOGIN_DENIED_BY_USER & 0xff;
    io_exchange(CHANNEL_APDU | IO_RETURN_AFTER_TX, 2);
    ui_idle_init();
}

void hello_login_confirm(void){
	io_exchange(CHANNEL_APDU | IO_RETURN_AFTER_TX, compute_login_reply());
    ui_idle_init();
}

