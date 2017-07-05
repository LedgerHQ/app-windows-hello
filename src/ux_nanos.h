#ifndef UX_NANOS_H
#define UX_NANOS_H

#include "ux_common.h"
#include "glyphs.h"

extern uint8_t icon_hack_flag;

void ui_idle_init(void);
void menu_settings_confirm_login_change_nanos(uint32_t confirm);
void menu_settings_dlock_change_nanos(uint32_t confirm);
void menu_settings_confirm_login_init_nanos(unsigned int ignored);
void menu_settings_dlock_init_nanos(unsigned int ignored);
void ui_confirm_login_init(void);
void ui_confirm_registration_init(void);
void icon_change_callback(unsigned int ignored);

#endif //UX_NANOS_H
