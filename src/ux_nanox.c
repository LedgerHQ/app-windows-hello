#if defined (TARGET_NANOX)

#include "ux_nanox.h"

extern unsigned int ux_step;
extern unsigned int ux_step_count;

////////////////////////////////////////////////////////////////
// Menu Settings
const char* const settings_general_submenu_getter_values[] = {
  "Auto-unlock",
  "Unplug to lock",
  "Back",
};

const char* settings_general_submenu_getter(unsigned int idx) {
  if (idx < ARRAYLEN(settings_general_submenu_getter_values)) {
    return settings_general_submenu_getter_values[idx];
  }
  return NULL;
}

void settings_general_submenu_selector(unsigned int idx) {
  switch(idx) {
    case 0:
      ui_auto_unlock_init();
      break;
    case 1:
      ui_unplug_to_lock_init();
      break;
    default:
      ui_idle_init();
  }
}

////////////////////////////////////////////////////////////////
// Confirm login flow
UX_FLOW_DEF_VALID(
  ux_confirm_login_flow_1_step,
  pbb,
  hello_login_confirm(),
  {
    &C_icon_validate_14,
    "Confirm",
    "registration",
  });
UX_FLOW_DEF_VALID(
  ux_confirm_login_flow_2_step,
  pbb,
  hello_login_cancel(),
  {
    &C_icon_crossmark,
    "Deny",
    "registration",
  });

const ux_flow_step_t *        const ux_confirm_login_flow [] = {
  &ux_confirm_login_flow_1_step,
  &ux_confirm_login_flow_2_step,
  FLOW_END_STEP,
}; 

////////////////////////////////////////////////////////////////
// Confirm registration flow
UX_FLOW_DEF_VALID(
  ux_confirm_registration_flow_1_step,
  pbb,
  hello_register_confirm(),
  {
    &C_icon_validate_14,
    "Confirm",
    "registration",
  });
UX_FLOW_DEF_VALID(
  ux_confirm_registration_flow_2_step,
  pbb,
  hello_register_cancel(),
  {
    &C_icon_crossmark,
    "Deny",
    "registration",
  });

const ux_flow_step_t *        const ux_confirm_registration_flow [] = {
  &ux_confirm_registration_flow_1_step,
  &ux_confirm_registration_flow_2_step,
  FLOW_END_STEP,
}; 


////////////////////////////////////////////////////////////////
// Ui idle flow
UX_FLOW_DEF_NOCB(
  ux_idle_flow_1_step, 
  pnn, //pnn, 
  {
    &C_icon_hello,
    "Ready",
    "to authenticate",
  });
UX_FLOW_DEF_VALID(
  ux_idle_flow_2_step, 
  pbb,
  ux_menulist_init(settings_general_submenu_getter, settings_general_submenu_selector),
  {
    &C_icon_coggle,
    "Settings",
    "",
  });
UX_FLOW_DEF_NOCB(
  ux_idle_flow_3_step, 
  bn, 
  {
    "Version",
    APPVERSION,
  });
UX_FLOW_DEF_VALID(
  ux_idle_flow_4_step,
  pb,
  os_sched_exit(-1),
  {
    &C_icon_dashboard,
    "Quit",
  });

const ux_flow_step_t *        const ux_idle_flow [] = {
  &ux_idle_flow_1_step,
  &ux_idle_flow_2_step,
  &ux_idle_flow_3_step,
  &ux_idle_flow_4_step,
  FLOW_END_STEP,
}; 

////////////////////////////////////////////////////////////////
// Sub menu auto-unlock
const char* const settings_submenu_auto_unlock_getter_values[] = {
  "Yes",
  "No",
  // "Back",
};

const char* settings_submenu_auto_unlock_getter(unsigned int idx) {
  if (idx < ARRAYLEN(settings_submenu_auto_unlock_getter_values)) {
    return settings_submenu_auto_unlock_getter_values[idx];
  }
  return NULL;
}

void settings_submenu_auto_unlock_selector(unsigned int idx) {
  switch(idx) {
    case 0:
      menu_settings_confirm_login_change_nanos(1);
      break;
    case 1:
      menu_settings_confirm_login_change_nanos(0);
      break;
    default:
      // ui_idle_init();
      ux_menulist_init(settings_general_submenu_getter, settings_general_submenu_selector);
      break;
  }
}

void menu_settings_confirm_login_change_nanos(uint32_t confirm) {
  nvm_write((void*)&N_storage.dont_confirm_login, (void*)&confirm, sizeof(uint32_t));
  // go back to the menu entry
  ux_menulist_init(settings_general_submenu_getter, settings_general_submenu_selector);
}

////////////////////////////////////////////////////////////////
// Sub menu unplug to lock
const char* const settings_submenu_unplug_to_lock_getter_values[] = {
  "Yes",
  "No",
  // "Back",
};

const char* settings_submenu_unplug_to_lock_getter(unsigned int idx) {
  if (idx < ARRAYLEN(settings_submenu_unplug_to_lock_getter_values)) {
    return settings_submenu_unplug_to_lock_getter_values[idx];
  }
  return NULL;
}

void settings_submenu_unplug_to_lock_selector(unsigned int idx) {
  switch(idx) {
    case 0:
      menu_settings_unplug_to_lock_change_nanos(0);
      break;
    case 1:
      menu_settings_unplug_to_lock_change_nanos(1);
      break;
    default:
      // ui_idle_init();
      ux_menulist_init(settings_general_submenu_getter, settings_general_submenu_selector);
      break;
  }
}

void menu_settings_unplug_to_lock_change_nanos(uint32_t confirm) {
  nvm_write((void*)&N_storage.dynamic_lock, (void*)&confirm, sizeof(uint32_t));
  // go back to the menu entry
  ux_menulist_init(settings_general_submenu_getter, settings_general_submenu_selector);
}

void ui_idle_init(void){
  ux_step_count = 0;
  if(G_ux.stack_count == 0) {
    ux_stack_push();
  }
  ux_flow_init(0, ux_idle_flow, NULL);
}

void ui_confirm_registration_init(void){
  ux_flow_init(0, ux_confirm_registration_flow, NULL);
}

void ui_confirm_login_init(void){
  ux_flow_init(0, ux_confirm_login_flow, NULL);
}

void ui_auto_unlock_init(void){
  ux_menulist_init_select(settings_submenu_auto_unlock_getter, settings_submenu_auto_unlock_selector, !N_storage.dont_confirm_login);
}

void ui_unplug_to_lock_init(void){
  // ux_flow_init(0, ux_unplug_to_lock_flow, NULL);
  ux_menulist_init_select(settings_submenu_unplug_to_lock_getter, settings_submenu_unplug_to_lock_selector, N_storage.dynamic_lock);
}

#endif // (TARGET_NANOX)