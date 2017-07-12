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

#ifndef UX_BLUE_H
#define UX_BLUE_H

#include "ux_common.h"
#include "glyphs.h"

#define BAGL_FONT_OPEN_SANS_LIGHT_16_22PX_AVG_WIDTH 10
#define BAGL_FONT_OPEN_SANS_REGULAR_10_13PX_AVG_WIDTH 8
#define MAX_CHAR_PER_LINE 25

#define COLOR_BG_1 0xF9F9F9
#define COLOR_APP 0x0ebdcf
#define COLOR_APP_LIGHT 0x87dee6

extern bagl_element_t tmp_element;

unsigned int io_seproxyhal_touch_settings(const bagl_element_t *e);
unsigned int io_seproxyhal_touch_exit(const bagl_element_t *e);
void ui_idle_init(void);
void ui_confirm_registration_init(void);
void ui_confirm_login_init(void);
unsigned int ui_settings_back_callback(const bagl_element_t* e);
const bagl_element_t * ui_settings_blue_toggle_confirm_login_blue(const bagl_element_t * e);
const bagl_element_t * ui_settings_blue_toggle_dlock_blue(const bagl_element_t * e);
// don't perform any draw/color change upon finger event over settings
const bagl_element_t* ui_settings_out_over(const bagl_element_t* e);
unsigned int ui_idle_mainmenu_blue_button(unsigned int button_mask, unsigned int button_mask_counter);
const bagl_element_t * ui_settings_blue_prepro(const bagl_element_t * e);
unsigned int ui_settings_blue_button(unsigned int button_mask, unsigned int button_mask_counter);
unsigned int io_seproxyhal_touch_settings(const bagl_element_t *e);
unsigned int io_seproxyhal_touch_exit(const bagl_element_t *e);
unsigned int hello_register_callback_cancel_blue(void);
unsigned int hello_register_callback_confirm_blue(void);
unsigned int hello_login_callback_cancel_blue(void);
unsigned int hello_login_callback_confirm_blue(void);


unsigned int ui_idle_mainmenu_list_blue_button(unsigned int button_mask, unsigned int button_mask_counter); // To remove for testing list
extern unsigned int list_idx;
unsigned int ui_list_text1_cb(const bagl_element_t *e);
unsigned int ui_list_text5_cb(const bagl_element_t *e);
const bagl_element_t * ui_idle_mainmenu_list_blue_prepro(const bagl_element_t * e);

#endif //UX_BLUE_H 
