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
#endif //UX_BLUE_H 
