/*******************************************************************************
*   Ledger Nano S - Secure firmware
*   (c) 2016, 2017 Ledger
*
*  Licensed under the Apache License, Version 2.0 (the "License");
*  you may not use this file except in compliance with the License.
*  You may obtain a copy of the License at
*
*      http://www.apache.org/licenses/LICENSE-2.0
*
*  Unless required by applicable law or agreed to in writing, software
*  distributed under the License is distributed on an "AS IS" BASIS,
*  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
*  See the License for the specific language governing permissions and
*  limitations under the License.
********************************************************************************/

#include "ux_common.h"

unsigned int ux_step;
unsigned int ux_step_count;
unsigned char device_key[32];
unsigned char auth_key[32];
unsigned int icon_change_timer_cnt;
uint8_t icon_change_click_cnt;
unsigned int icon_change_timer_cnt_t1;
unsigned int icon_change_timer_cnt_t10;


/*typedef struct internalStorage_t {
// #define STORAGE_MAGIC 0xDEAD1337
//     uint32_t magic;
    uint32_t dont_confirm_login;
    uint32_t dynamic_lock;
} internalStorage_t;*/

WIDE internalStorage_t const N_storage_real;
#define N_storage (*(volatile internalStorage_t *)PIC(&N_storage_real))

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

