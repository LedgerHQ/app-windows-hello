#ifndef UX_FLOW_H
#define UX_FLOW_H

#include "ux_common.h"

#if defined (HAVE_UX_FLOW)

#include "ux.h"
#include "ux_layouts.h"

void ui_idle_init(void);
void ui_confirm_registration_init(void);
void ui_confirm_login_init(void);
void menu_settings_confirm_login_change_nanos(uint32_t confirm);
void menu_settings_unplug_to_lock_change_nanos(uint32_t confirm);
void ui_auto_unlock_init(void);
void ui_unplug_to_lock_init(void);
void settings_submenu_auto_unlock_selector(unsigned int idx);
void settings_submenu_unplug_to_lock_selector(unsigned int idx);
const char* settings_submenu_auto_unlock_getter(unsigned int idx);
const char* settings_submenu_unplug_to_lock_getter(unsigned int idx);

#endif //HAVE_UX_FLOW
#endif //UX_FLOW_H
