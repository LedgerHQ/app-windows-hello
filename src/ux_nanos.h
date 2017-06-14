#ifndef UX_NANOS_H
#define UX_NANOS_H

#include "ux_common.h"
#include "glyphs.h"
 
const bagl_element_t ui_confirm_registration_nanos[] = {
    // type                               userid    x    y   w    h  str rad
    // fill      fg        bg      fid iid  txt   touchparams...       ]
    {{BAGL_RECTANGLE, 0x00, 0, 0, 128, 32, 0, 0, BAGL_FILL, 0x000000, 0xFFFFFF,
      0, 0},
     NULL,
     0,
     0,
     0,
     NULL,
     NULL,
     NULL},

    {{BAGL_LABELINE, 0x01, 33, 12, 128, 32, 0, 0, 0, 0xFFFFFF, 0x000000,
      BAGL_FONT_OPEN_SANS_EXTRABOLD_11px, 0},
     "Confirm",
     0,
     0,
     0,
     NULL,
     NULL,
     NULL},
    {{BAGL_LABELINE, 0x01, 34, 26, 128, 32, 0, 0, 0, 0xFFFFFF, 0x000000,
      BAGL_FONT_OPEN_SANS_EXTRABOLD_11px, 0},
     "Registration",
     0,
     0,
     0,
     NULL,
     NULL,
     NULL},

    {{BAGL_ICON, 0x00, 3, 12, 7, 7, 0, 0, 0, 0xFFFFFF, 0x000000, 0,
      BAGL_GLYPH_ICON_CROSS},
     NULL,
     0,
     0,
     0,
     NULL,
     NULL,
     NULL},

    {{BAGL_ICON, 0x00, 117, 13, 8, 6, 0, 0, 0, 0xFFFFFF, 0x000000, 0,
      BAGL_GLYPH_ICON_CHECK},
     NULL,
     0,
     0,
     0,
     NULL,
     NULL,
     NULL},

};

const bagl_element_t ui_confirm_login_nanos[] = {
    // type                               userid    x    y   w    h  str rad
    // fill      fg        bg      fid iid  txt   touchparams...       ]
    {{BAGL_RECTANGLE, 0x00, 0, 0, 128, 32, 0, 0, BAGL_FILL, 0x000000, 0xFFFFFF,
      0, 0},
     NULL,
     0,
     0,
     0,
     NULL,
     NULL,
     NULL},

    {{BAGL_LABELINE, 0x01, 33, 12, 128, 32, 0, 0, 0, 0xFFFFFF, 0x000000,
      BAGL_FONT_OPEN_SANS_EXTRABOLD_11px, 0},
     "Confirm",
     0,
     0,
     0,
     NULL,
     NULL,
     NULL},
    {{BAGL_LABELINE, 0x01, 34, 26, 128, 32, 0, 0, 0, 0xFFFFFF, 0x000000,
      BAGL_FONT_OPEN_SANS_EXTRABOLD_11px, 0},
     "Log-in",
     0,
     0,
     0,
     NULL,
     NULL,
     NULL},

    {{BAGL_ICON, 0x00, 3, 12, 7, 7, 0, 0, 0, 0xFFFFFF, 0x000000, 0,
      BAGL_GLYPH_ICON_CROSS},
     NULL,
     0,
     0,
     0,
     NULL,
     NULL,
     NULL},

    {{BAGL_ICON, 0x00, 117, 13, 8, 6, 0, 0, 0, 0xFFFFFF, 0x000000, 0,
      BAGL_GLYPH_ICON_CHECK},
     NULL,
     0,
     0,
     0,
     NULL,
     NULL,
     NULL},

};


const ux_menu_entry_t ui_idle_mainmenu_nanos[];
const ux_menu_entry_t menu_settings_nanos[];

const ux_menu_entry_t menu_about_nanos[] = {
    {NULL, NULL, 0, NULL, "Version", APPVERSION, 0, 0},
    {ui_idle_mainmenu_nanos, NULL, 1, &C_icon_back, "Back", NULL, 61, 40},
    UX_MENU_END}; 

const ux_menu_entry_t ui_idle_mainmenu_nanos[] = {
  {NULL, NULL, 0, &C_icon_hello, "Ready to", "authenticate", 37, 11},
  {menu_settings_nanos, NULL, 0, NULL, "Settings", NULL, 0, 0},
  {menu_about_nanos, NULL, 0, NULL, "About", NULL, 0, 0},
  {NULL, os_sched_exit, 0, &C_icon_dashboard, "Quit app", NULL, 50, 29},
  UX_MENU_END
};



// change the setting
void menu_settings_confirm_login_change_nanos(uint32_t confirm) {
  nvm_write(&N_storage.dont_confirm_login, (void*)&confirm, sizeof(uint32_t));
  // go back to the menu entry
  UX_MENU_DISPLAY(0, menu_settings_nanos, NULL);
}

// change the setting
void menu_settings_dlock_change_nanos(uint32_t confirm) {
  nvm_write(&N_storage.dynamic_lock, (void*)&confirm, sizeof(uint32_t));
  // go back to the menu entry
  UX_MENU_DISPLAY(1, menu_settings_nanos, NULL);
  }

const ux_menu_entry_t menu_settings_confirm_login_nanos[] = {
  {NULL, menu_settings_confirm_login_change_nanos, 1, NULL, "No", NULL, 0, 0},
  {NULL, menu_settings_confirm_login_change_nanos, 0, NULL, "Yes", NULL, 0, 0},
  UX_MENU_END
};

const ux_menu_entry_t menu_settings_dlock_nanos[] = {
  {NULL, menu_settings_dlock_change_nanos, 0, NULL, "No", NULL, 0, 0},
  {NULL, menu_settings_dlock_change_nanos, 1, NULL, "Yes", NULL, 0, 0},
  UX_MENU_END
};

// show the currently activated entry
void menu_settings_confirm_login_init_nanos(unsigned int ignored) {
  UNUSED(ignored);
  UX_MENU_DISPLAY(!N_storage.dont_confirm_login, menu_settings_confirm_login_nanos, NULL);
}

// show the currently activated entry
void menu_settings_dlock_init_nanos(unsigned int ignored) {
  UNUSED(ignored);
  UX_MENU_DISPLAY(N_storage.dynamic_lock, menu_settings_dlock_nanos, NULL);
}

const ux_menu_entry_t menu_settings_nanos[] = {
  {NULL, menu_settings_confirm_login_init_nanos, 0, NULL, "Confirm login", NULL, 0, 0},
  {NULL, menu_settings_dlock_init_nanos, 0, NULL, "Lock when unplugged", NULL, 0, 0},
  {ui_idle_mainmenu_nanos, NULL, 0, &C_icon_back, "Back", NULL, 61, 40},
  UX_MENU_END
};

#endif //UX_NANOS_H
