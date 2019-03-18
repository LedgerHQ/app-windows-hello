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

#include "ux_nanos.h"

#if defined (TARGET_NANOS)

#define ICON_CHANGE_MAX_INTERVAL 40

extern const bagl_element_t ui_confirm_registration_nanos[];
extern const bagl_element_t ui_confirm_login_nanos[];
extern const ux_menu_entry_t ui_idle_mainmenu_nanos[];
extern const ux_menu_entry_t menu_settings_nanos[];
extern const ux_menu_entry_t menu_settings_confirm_login_nanos[];
#ifdef DYNAMIC_LOCK
extern const ux_menu_entry_t menu_settings_dlock_nanos[];
#endif
extern const ux_menu_entry_t menu_settings_nanos[];

bagl_icon_details_t icon_hack;
uint8_t icon_hack_flag;

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

    {{BAGL_LABELINE, 0x01, 0, 12, 128, 32, 0, 0, 0, 0xFFFFFF, 0x000000,
      BAGL_FONT_OPEN_SANS_EXTRABOLD_11px | BAGL_FONT_ALIGNMENT_CENTER , 0},
     "Confirm",
     0,
     0,
     0,
     NULL,
     NULL,
     NULL},
    {{BAGL_LABELINE, 0x01, 1, 26, 128, 32, 0, 0, 0, 0xFFFFFF, 0x000000,
      BAGL_FONT_OPEN_SANS_EXTRABOLD_11px | BAGL_FONT_ALIGNMENT_CENTER , 0},
     "registration",
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

    {{BAGL_LABELINE, 0x01, 0, 12, 128, 32, 0, 0, 0, 0xFFFFFF, 0x000000,
      BAGL_FONT_OPEN_SANS_EXTRABOLD_11px | BAGL_FONT_ALIGNMENT_CENTER , 0},
     "Confirm",
     0,
     0,
     0,
     NULL,
     NULL,
     NULL},
    {{BAGL_LABELINE, 0x01, 1, 26, 128, 32, 0, 0, 0, 0xFFFFFF, 0x000000,
      BAGL_FONT_OPEN_SANS_EXTRABOLD_11px | BAGL_FONT_ALIGNMENT_CENTER , 0},
     "authentication",
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

const ux_menu_entry_t menu_about_nanos[] = {
    {NULL, NULL, 0, NULL, "Version", APPVERSION, 0, 0},
    {ui_idle_mainmenu_nanos, NULL, 1, &C_icon_back, "Back", NULL, 61, 40},
    UX_MENU_END}; 

const ux_menu_entry_t ui_idle_mainmenu_nanos[] = {
  {NULL, icon_change_callback, 0, &icon_hack, "Ready to", "authenticate", 32, 10},
  {menu_settings_nanos, NULL, 0, NULL, "Settings", NULL, 0, 0},
  {menu_about_nanos, NULL, 0, NULL, "About", NULL, 0, 0},
  {NULL, os_sched_exit, 0, &C_icon_dashboard, "Quit app", NULL, 50, 29},
  UX_MENU_END
};

const ux_menu_entry_t menu_settings_nanos[] = {
  {NULL, menu_settings_confirm_login_init_nanos, 0, NULL, "Auto-unlock", NULL, 0, 0},
  #ifdef DYNAMIC_LOCK
    {NULL, menu_settings_dlock_init_nanos, 0, NULL, "Unplug to lock", NULL, 0, 0},
  #endif
  {ui_idle_mainmenu_nanos, NULL, 0, &C_icon_back, "Back", NULL, 61, 40},
  UX_MENU_END
};

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

void icon_change_callback(unsigned int ignored){
	UNUSED(ignored);

	icon_change_click_cnt++;

	if (icon_change_click_cnt == 1){
    icon_change_timer_cnt_t1 = icon_change_timer_cnt = 0;
	}

	if (icon_change_click_cnt == 10){
		icon_change_click_cnt = 0;
		icon_change_timer_cnt_t10 = icon_change_timer_cnt;

		if (icon_change_timer_cnt_t10 - icon_change_timer_cnt_t1 < ICON_CHANGE_MAX_INTERVAL){
			//Change icon
			if (icon_hack_flag == 0){
				icon_hack = C_icon_hello_old;
				icon_hack_flag = 1;
			}
			else if (icon_hack_flag == 1){
				icon_hack = C_icon_pirate;
				icon_hack_flag = 2;
			}
      else {
        icon_hack = C_icon_hello;
        icon_hack_flag = 0;
      }
			
			UX_MENU_DISPLAY(0, ui_idle_mainmenu_nanos, NULL);
  			// setup the first screen changing
  		UX_CALLBACK_SET_INTERVAL(1000);
		}
	}
}

unsigned int ui_confirm_login_nanos_button(unsigned int button_mask,unsigned int button_mask_counter) {
    switch (button_mask) {
    case BUTTON_EVT_RELEASED | BUTTON_RIGHT:       
        hello_login_confirm();
        //hello_register_confirm();
        break;

    case BUTTON_EVT_RELEASED | BUTTON_LEFT:
        // deny
        hello_login_cancel();
        break;

    default:
        break;
    }
}

unsigned int ui_confirm_registration_nanos_button(unsigned int button_mask,unsigned int button_mask_counter) {
    uint8_t replySize;
    switch (button_mask) {
    case BUTTON_EVT_RELEASED | BUTTON_RIGHT:
        // confirm
        hello_register_confirm();
        break;

    case BUTTON_EVT_RELEASED | BUTTON_LEFT:
        // deny
        hello_register_cancel();
        break;

    default:
        break;
    }
}

// change the setting
void menu_settings_confirm_login_change_nanos(uint32_t confirm) {
  nvm_write(&N_storage.dont_confirm_login, (void*)&confirm, sizeof(uint32_t));
  // go back to the menu entry
  UX_MENU_DISPLAY(0, menu_settings_nanos, NULL);
}

#ifdef DYNAMIC_LOCK
// change the setting
void menu_settings_dlock_change_nanos(uint32_t confirm) {
  nvm_write(&N_storage.dynamic_lock, (void*)&confirm, sizeof(uint32_t));
  // go back to the menu entry
  UX_MENU_DISPLAY(1, menu_settings_nanos, NULL);
  }
#endif

// show the currently activated entry
void menu_settings_confirm_login_init_nanos(unsigned int ignored) {
  UNUSED(ignored);
  UX_MENU_DISPLAY(!N_storage.dont_confirm_login, menu_settings_confirm_login_nanos, NULL);
}

#ifdef DYNAMIC_LOCK
// show the currently activated entry
void menu_settings_dlock_init_nanos(unsigned int ignored) {
  UNUSED(ignored);
  UX_MENU_DISPLAY(N_storage.dynamic_lock, menu_settings_dlock_nanos, NULL);
}
#endif

void ui_confirm_registration_init(void){
	UX_DISPLAY(ui_confirm_registration_nanos,NULL);
}

void ui_confirm_login_init(void){
	UX_DISPLAY(ui_confirm_login_nanos,NULL);
}

void ui_idle_init(void) {
  ux_step = 0;
  ux_step_count = 2;

  if (icon_hack_flag == 0){
  	icon_hack = C_icon_hello;
  }
  else if (icon_hack_flag == 1){
    icon_hack = C_icon_hello_old;
  }
  else{
    icon_hack = C_icon_pirate;
  }
  
  icon_change_click_cnt = 0;

  UX_MENU_DISPLAY(0, ui_idle_mainmenu_nanos, NULL);
  // setup the first screen changing
  UX_CALLBACK_SET_INTERVAL(1000);
}

#endif
