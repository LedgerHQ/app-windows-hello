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

#include "ux_blue.h"

#if defined (TARGET_BLUE)

extern const bagl_element_t ui_settings_blue[];
extern const bagl_element_t ui_idle_mainmenu_blue[];

bagl_element_t tmp_element;

const bagl_element_t ui_settings_blue[] = {
  // type                               userid    x    y   w    h  str rad fill      fg        bg      fid iid  txt   touchparams...       ]
  {{BAGL_RECTANGLE                      , 0x00,   0,  68, 320, 413, 0, 0, BAGL_FILL, COLOR_BG_1, 0x000000, 0                                                                                 , 0   }, NULL, 0, 0, 0, NULL, NULL, NULL },

  // erase screen (only under the status bar)
  {{BAGL_RECTANGLE                      , 0x00,   0,  20, 320,  48, 0, 0, BAGL_FILL, COLOR_APP, COLOR_APP, 0                                                      , 0   }, NULL, 0, 0, 0, NULL, NULL, NULL},

  /// TOP STATUS BAR
  {{BAGL_LABELINE                       , 0x00,   0,  45, 320,  30, 0, 0, BAGL_FILL, 0xFFFFFF, COLOR_APP, BAGL_FONT_OPEN_SANS_SEMIBOLD_10_13PX|BAGL_FONT_ALIGNMENT_CENTER, 0   }, "SETTINGS", 0, 0, 0, NULL, NULL, NULL},

  {{BAGL_RECTANGLE | BAGL_FLAG_TOUCHABLE, 0x00,   0,  19,  50,  44, 0, 0, BAGL_FILL, COLOR_APP, COLOR_APP_LIGHT, BAGL_FONT_SYMBOLS_0|BAGL_FONT_ALIGNMENT_CENTER|BAGL_FONT_ALIGNMENT_MIDDLE, 0 }, BAGL_FONT_SYMBOLS_0_LEFT, 0, COLOR_APP, 0xFFFFFF, ui_settings_back_callback, NULL, NULL},
  {{BAGL_RECTANGLE | BAGL_FLAG_TOUCHABLE, 0x00, 264,  19,  56,  44, 0, 0, BAGL_FILL, COLOR_APP, COLOR_APP_LIGHT, BAGL_FONT_SYMBOLS_0|BAGL_FONT_ALIGNMENT_CENTER|BAGL_FONT_ALIGNMENT_MIDDLE, 0 }, BAGL_FONT_SYMBOLS_0_DASHBOARD, 0, COLOR_APP, 0xFFFFFF, io_seproxyhal_touch_exit, NULL, NULL},


  {{BAGL_LABELINE                       , 0x00,  30, 105, 160,  30, 0, 0, BAGL_FILL, 0x000000, COLOR_BG_1, BAGL_FONT_OPEN_SANS_REGULAR_10_13PX, 0   }, "Confirm login", 0, 0, 0, NULL, NULL, NULL},
  {{BAGL_LABELINE                       , 0x00,  30, 126, 260,  30, 0, 0, BAGL_FILL, 0x999999, COLOR_BG_1, BAGL_FONT_OPEN_SANS_REGULAR_8_11PX, 0   }, "Enable user consent for login", 0, 0, 0, NULL, NULL, NULL},
  {{BAGL_NONE   | BAGL_FLAG_TOUCHABLE   , 0x00,   0,  78, 320,  68, 0, 0, BAGL_FILL, 0xFFFFFF, 0x000000, 0                                                                                        , 0   }, NULL, 0, 0xEEEEEE, 0x000000, ui_settings_blue_toggle_confirm_login_blue, ui_settings_out_over, ui_settings_out_over },

  {{BAGL_RECTANGLE                      , 0x00,  30, 146, 260,   1, 1, 0, 0        , 0xEEEEEE, COLOR_BG_1, 0                                                                                    , 0   }, NULL, 0, 0, 0, NULL, NULL, NULL },
  #ifdef DYNAMIC_LOCK
  {{BAGL_LABELINE                       , 0x00,  30, 174, 160,  30, 0, 0, BAGL_FILL, 0x000000, COLOR_BG_1, BAGL_FONT_OPEN_SANS_REGULAR_10_13PX, 0   }, "Unplug to lock", 0, 0, 0, NULL, NULL, NULL},
  {{BAGL_LABELINE                       , 0x00,  30, 195, 260,  30, 0, 0, BAGL_FILL, 0x999999, COLOR_BG_1, BAGL_FONT_OPEN_SANS_REGULAR_8_11PX, 0   }, "Enable dynamic lock feature", 0, 0, 0, NULL, NULL, NULL},
  {{BAGL_NONE   | BAGL_FLAG_TOUCHABLE   , 0x00,   0, 147, 320,  68, 0, 0, BAGL_FILL, 0xFFFFFF, 0x000000, 0                                                                                        , 0   }, NULL, 0, 0xEEEEEE, 0x000000, ui_settings_blue_toggle_dlock_blue, ui_settings_out_over, ui_settings_out_over },
  #endif
  // at the end to minimize the number of refreshed items upon setting change
  {{BAGL_ICON                           , 0x02, 258, 167,  32,  18, 0, 0, BAGL_FILL, 0x000000, COLOR_BG_1, 0, 0   }, NULL, 0, 0, 0, NULL, NULL, NULL},
  {{BAGL_ICON                           , 0x01, 258,  98,  32,  18, 0, 0, BAGL_FILL, 0x000000, COLOR_BG_1, 0, 0   }, NULL, 0, 0, 0, NULL, NULL, NULL},
};

/// TODO remove : list test

// extern const bagl_element_t ui_idle_mainmenu_list_blue[];
// unsigned int list_idx;
// unsigned int sel_idx;

// #define LIST_POSITION 80
// #define LINE_SIZE 50
// #define FONT_UP_DOWN BAGL_FONT_OPEN_SANS_REGULAR_8_11PX
// #define SIZE_UP_DOWN  8
// #define FONT_LIST_ELEM  BAGL_FONT_OPEN_SANS_REGULAR_11_14PX
// #define SIZE_LIST_ELEM  11
// #define RECT_SEL_HEIGHT LINE_SIZE - 12
// #define LIST_LEN  9

// const char * const ui_list_text[LIST_LEN] = {"Text1","Text2","Text3","Text4","Text5","Text6","Text7","Text8","Text9"};


// const bagl_element_t * ui_idle_mainmenu_list_blue_prepro(const bagl_element_t * e) {
//   uint8_t idx;
//   os_memmove(&tmp_element, e, sizeof(bagl_element_t));
//   if (e->component.userid == 0) {
//     return 1;
//   }
//   if ((e->component.type&(~BAGL_FLAG_TOUCHABLE)) == BAGL_NONE) {
//     return 0;
//   }
//   else {        
//     switch (e->component.userid&0xF0){
//       case 0x10:
//         switch (e->component.userid&0x0F){
//           case 0x00:
//             if (list_idx == 0){
//               //tmp_element.text = " ";
//               return 0;
//             }
//             else {
//               tmp_element.text = "UP";
//             }
//             break;
//           case 0x01:
//             if ((LIST_LEN < 3)||(list_idx == LIST_LEN-3)){
//               //tmp_element.text = " ";
//               return 0;
//             }
//             else {
//               tmp_element.text = "DOWN";
//             }
//             break;
//         }
//         break;
//       case 0x40: // Selection rectangle

//         if ((e->component.userid&0x0F)>=LIST_LEN){
//           return 0;
//         }
//         break;
//       case 0x20:
//         if ((e->component.userid&0x0F)>=LIST_LEN){
//           return 0;
//         }
//         switch (e->component.userid&0x0F){
//           case 0:            
//             idx = list_idx;
//             break;
//           case 1:
//             idx = list_idx+1;
//             break;
//           case 2:
//             idx = list_idx+2;
//             break;          
//         }
//         tmp_element.text = ui_list_text[idx];        
//       break;
//     }
//   }
//   return &tmp_element;
// }


// unsigned int ui_list_item_out_over(const bagl_element_t *e) {  
//   // bagl_element_t *last_e = (const bagl_element_t*)(((unsigned int)e)-sizeof(bagl_element_t));
//   // if (last_e->text == NULL){
//   //   return 0;
//   // }
//   // else {
//   //   e = (const bagl_element_t*)(((unsigned int)e)+sizeof(bagl_element_t));
//   //   return e;
//   // }

//   e = (const bagl_element_t*)(((unsigned int)e)+sizeof(bagl_element_t));
//   return e;
// }

// unsigned int ui_list_item_tap(const bagl_element_t *e) {
//   return 0;
// }

// unsigned int ui_list_up_cb(const bagl_element_t *e) {
//   if ((LIST_LEN > 3)&&(list_idx > 0)){
//     list_idx--;
//     UX_REDISPLAY_IDX(5);
//     return 0;
//   }    
//   return 1;
// }

// unsigned int ui_list_down_cb(const bagl_element_t *e) {
//   if ((LIST_LEN > 3)&&(list_idx < LIST_LEN-3)){
//     list_idx++;
//     //UX_REDISPLAY();
//     UX_REDISPLAY_IDX(5);
//     return 0;
//   }  
//   return 1;
// }
// const bagl_element_t ui_idle_mainmenu_list_blue[] = {
//   // type                               userid    x    y   w    h  str rad fill      fg        bg      fid iid  txt   touchparams...       ]
//   {{BAGL_RECTANGLE                      , 0x00,   0,  68, 320, 413, 0, 0, BAGL_FILL, COLOR_BG_1, 0x000000, 0                                                                                 , 0   }, NULL, 0, 0, 0, NULL, NULL, NULL },

//   // erase screen (only under the status bar)
//   {{BAGL_RECTANGLE                      , 0x00,   0,  20, 320,  48, 0, 0, BAGL_FILL, COLOR_APP, COLOR_APP, 0                                                      , 0   }, NULL, 0, 0, 0, NULL, NULL, NULL},

//   /// TOP STATUS BAR
//   {{BAGL_LABELINE                       , 0x00,   0,  45, 320,  30, 0, 0, BAGL_FILL, 0xFFFFFF, COLOR_APP, BAGL_FONT_OPEN_SANS_SEMIBOLD_10_13PX|BAGL_FONT_ALIGNMENT_CENTER, 0   }, "APP NAME", 0, 0, 0, NULL, NULL, NULL},

//   {{BAGL_RECTANGLE | BAGL_FLAG_TOUCHABLE, 0x00,   0,  19,  56,  44, 0, 0, BAGL_FILL, COLOR_APP, COLOR_APP_LIGHT, BAGL_FONT_SYMBOLS_0|BAGL_FONT_ALIGNMENT_CENTER|BAGL_FONT_ALIGNMENT_MIDDLE, 0 }, BAGL_FONT_SYMBOLS_0_SETTINGS, 0, COLOR_APP, 0xFFFFFF, io_seproxyhal_touch_settings, NULL, NULL},
//   {{BAGL_RECTANGLE | BAGL_FLAG_TOUCHABLE, 0x00, 264,  19,  56,  44, 0, 0, BAGL_FILL, COLOR_APP, COLOR_APP_LIGHT, BAGL_FONT_SYMBOLS_0|BAGL_FONT_ALIGNMENT_CENTER|BAGL_FONT_ALIGNMENT_MIDDLE, 0 }, BAGL_FONT_SYMBOLS_0_DASHBOARD, 0, COLOR_APP, 0xFFFFFF, io_seproxyhal_touch_exit, NULL, NULL},

  
  
  
//   // Display List UP
//   {{BAGL_RECTANGLE                      , 0x30,  130, LIST_POSITION - (SIZE_LIST_ELEM + 1 )            , 60,  LINE_SIZE, 0, 0, BAGL_FILL, COLOR_BG_1, COLOR_BG_1, NULL                                                                                   , 0   }, NULL, 0, 0, 0, NULL, NULL, NULL},
//   {{BAGL_LABELINE| BAGL_FLAG_TOUCHABLE  , 0x10,  130, LIST_POSITION                                    , 60,  LINE_SIZE, 0, 0, BAGL_FILL, 0x707070, COLOR_BG_1, FONT_UP_DOWN|BAGL_FONT_ALIGNMENT_CENTER|BAGL_FONT_ALIGNMENT_MIDDLE  , 0   }, NULL, 0, 0, COLOR_BG_1, ui_list_up_cb, NULL, NULL},
//   // Display List DOWN
//   {{BAGL_RECTANGLE                      , 0x31,  130, LIST_POSITION+4*LINE_SIZE - (SIZE_LIST_ELEM + 1 ), 60,  LINE_SIZE, 0, 0, BAGL_FILL, COLOR_BG_1, COLOR_BG_1, NULL                                                                                   , 0   }, NULL, 0, 0, 0, NULL, NULL, NULL},
//   {{BAGL_LABELINE| BAGL_FLAG_TOUCHABLE  , 0x11,  130, LIST_POSITION+4*LINE_SIZE                        , 60,  LINE_SIZE, 0, 0, BAGL_FILL, 0x707070, COLOR_BG_1, FONT_UP_DOWN|BAGL_FONT_ALIGNMENT_CENTER|BAGL_FONT_ALIGNMENT_MIDDLE  , 0   }, NULL, 0, 0, COLOR_BG_1, ui_list_down_cb, NULL, NULL},

//   // Display list Element 1
//   {{BAGL_LABELINE                       , 0x20,  130, LIST_POSITION+LINE_SIZE                          , 60,  LINE_SIZE, 0, 0, BAGL_FILL, 0x000000, COLOR_BG_1, FONT_LIST_ELEM|BAGL_FONT_ALIGNMENT_CENTER|BAGL_FONT_ALIGNMENT_MIDDLE, 0   }, NULL, 0, 0, 0, NULL, NULL, NULL},  
//   {{BAGL_NONE   | BAGL_FLAG_TOUCHABLE   , 0x00,  0  , LIST_POSITION+LINE_SIZE-(2*SIZE_LIST_ELEM)   , 320 , RECT_SEL_HEIGHT, 0, 0, BAGL_FILL, 0xFFFFFF, 0x000000 , 0 , 0   }, NULL, 0, 0xEEEEEE, 0x000000, ui_list_item_tap, ui_list_item_out_over, ui_list_item_out_over},
//   {{BAGL_RECTANGLE                      , 0x40,  0  , LIST_POSITION+LINE_SIZE-(2*SIZE_LIST_ELEM)   , 5   , RECT_SEL_HEIGHT, 0, 0, BAGL_FILL, COLOR_BG_1, COLOR_BG_1 , FONT_LIST_ELEM|BAGL_FONT_ALIGNMENT_CENTER|BAGL_FONT_ALIGNMENT_MIDDLE, 0   }, NULL, 0, COLOR_APP, 0, NULL, NULL, NULL},  

//   // Display list Element 2
//   {{BAGL_LABELINE                       , 0x21,  130, LIST_POSITION+2*LINE_SIZE                        , 60  ,  LINE_SIZE, 0, 0, BAGL_FILL, 0x000000, COLOR_BG_1, FONT_LIST_ELEM|BAGL_FONT_ALIGNMENT_CENTER|BAGL_FONT_ALIGNMENT_MIDDLE, 0   }, NULL, 0, 0, 0, NULL, NULL, NULL},
//   {{BAGL_NONE   | BAGL_FLAG_TOUCHABLE   , 0x00,  0  , LIST_POSITION+2*LINE_SIZE-(2*SIZE_LIST_ELEM) , 320 , RECT_SEL_HEIGHT, 0, 0, BAGL_FILL, 0xFFFFFF, 0x000000 , 0 , 0   }, NULL, 0, 0xEEEEEE, 0x000000, ui_list_item_tap, ui_list_item_out_over, ui_list_item_out_over},
//   {{BAGL_RECTANGLE                      , 0x41,  0  , LIST_POSITION+2*LINE_SIZE-(2*SIZE_LIST_ELEM) , 5   , RECT_SEL_HEIGHT, 0, 0, BAGL_FILL, COLOR_BG_1, COLOR_BG_1 , FONT_LIST_ELEM|BAGL_FONT_ALIGNMENT_CENTER|BAGL_FONT_ALIGNMENT_MIDDLE, 0   }, NULL, 0, COLOR_APP, 0, NULL, NULL, NULL},

//   // Display list Element 3
//   {{BAGL_LABELINE                       , 0x22,  130, LIST_POSITION+3*LINE_SIZE                        , 60,  LINE_SIZE, 0, 0, BAGL_FILL, 0x000000, COLOR_BG_1, FONT_LIST_ELEM|BAGL_FONT_ALIGNMENT_CENTER|BAGL_FONT_ALIGNMENT_MIDDLE, 0   }, NULL, 0, 0, 0, NULL, NULL, NULL},
//   {{BAGL_NONE   | BAGL_FLAG_TOUCHABLE   , 0x00,  0  , LIST_POSITION+3*LINE_SIZE-(2*SIZE_LIST_ELEM) , 320 , RECT_SEL_HEIGHT, 0, 0, BAGL_FILL, 0xFFFFFF, 0x000000 , 0 , 0   }, NULL, 0, 0xEEEEEE, 0x000000, ui_list_item_tap, ui_list_item_out_over, ui_list_item_out_over},
//   {{BAGL_RECTANGLE                      , 0x42,  0  , LIST_POSITION+3*LINE_SIZE-(2*SIZE_LIST_ELEM) , 5 , RECT_SEL_HEIGHT, 0, 0, BAGL_FILL, COLOR_BG_1, COLOR_BG_1 , FONT_LIST_ELEM|BAGL_FONT_ALIGNMENT_CENTER|BAGL_FONT_ALIGNMENT_MIDDLE, 0   }, NULL, 0, COLOR_APP, 0, NULL, NULL, NULL},
// };


const bagl_element_t ui_idle_mainmenu_blue[] = {
  // type                               userid    x    y   w    h  str rad fill      fg        bg      fid iid  txt   touchparams...       ]
  {{BAGL_RECTANGLE                      , 0x00,   0,  68, 320, 413, 0, 0, BAGL_FILL, COLOR_BG_1, 0x000000, 0                                                                                 , 0   }, NULL, 0, 0, 0, NULL, NULL, NULL },

  // erase screen (only under the status bar)
  {{BAGL_RECTANGLE                      , 0x00,   0,  20, 320,  48, 0, 0, BAGL_FILL, COLOR_APP, COLOR_APP, 0                                                      , 0   }, NULL, 0, 0, 0, NULL, NULL, NULL},

  /// TOP STATUS BAR
  {{BAGL_LABELINE                       , 0x00,   0,  45, 320,  30, 0, 0, BAGL_FILL, 0xFFFFFF, COLOR_APP, BAGL_FONT_OPEN_SANS_SEMIBOLD_10_13PX|BAGL_FONT_ALIGNMENT_CENTER, 0   }, "WINDOWS HELLO", 0, 0, 0, NULL, NULL, NULL},

  {{BAGL_RECTANGLE | BAGL_FLAG_TOUCHABLE, 0x00,   0,  19,  56,  44, 0, 0, BAGL_FILL, COLOR_APP, COLOR_APP_LIGHT, BAGL_FONT_SYMBOLS_0|BAGL_FONT_ALIGNMENT_CENTER|BAGL_FONT_ALIGNMENT_MIDDLE, 0 }, BAGL_FONT_SYMBOLS_0_SETTINGS, 0, COLOR_APP, 0xFFFFFF, io_seproxyhal_touch_settings, NULL, NULL},
  {{BAGL_RECTANGLE | BAGL_FLAG_TOUCHABLE, 0x00, 264,  19,  56,  44, 0, 0, BAGL_FILL, COLOR_APP, COLOR_APP_LIGHT, BAGL_FONT_SYMBOLS_0|BAGL_FONT_ALIGNMENT_CENTER|BAGL_FONT_ALIGNMENT_MIDDLE, 0 }, BAGL_FONT_SYMBOLS_0_DASHBOARD, 0, COLOR_APP, 0xFFFFFF, io_seproxyhal_touch_exit, NULL, NULL},

  // BADGE_ETHEREUM.GIF
  //{{BAGL_ICON                           , 0x00, 135, 178,  50,  50, 0, 0, BAGL_FILL, 0       , COLOR_BG_1, 0                                                              ,0   } , &C_badge_ethereum, 0, 0, 0, NULL, NULL, NULL },

  {{BAGL_LABELINE                       , 0x00,   0, 270, 320,  30, 0, 0, BAGL_FILL, 0x000000, COLOR_BG_1, BAGL_FONT_OPEN_SANS_LIGHT_16_22PX|BAGL_FONT_ALIGNMENT_CENTER, 0   }, "Ready to authenticate", 0, 0, 0, NULL, NULL, NULL},
};

const bagl_element_t ui_confirm_registration_blue[] = {
    {{BAGL_RECTANGLE, 0x00, 0, 68, 320, 413, 0, 0, BAGL_FILL, COLOR_BG_1,
      0x000000, 0, 0},
     NULL,
     0,
     0,
     0,
     NULL,
     NULL,
     NULL},

    // erase screen (only under the status bar)
    {{BAGL_RECTANGLE, 0x00, 0, 20, 320, 48, 0, 0, BAGL_FILL, COLOR_APP,
      COLOR_APP, 0, 0},
     NULL,
     0,
     0,
     0,
     NULL,
     NULL,
     NULL},

    /// TOP STATUS BAR
    {{BAGL_LABELINE, 0x30, 0, 45, 320, 30, 0, 0, BAGL_FILL, 0xFFFFFF, COLOR_APP,
      BAGL_FONT_OPEN_SANS_SEMIBOLD_10_13PX | BAGL_FONT_ALIGNMENT_CENTER, 0},
     "WINDOWS HELLO",
     0,
     0,
     0,
     NULL,
     NULL,
     NULL},

    // BADGE_LOCK_BLUE.GIF
    /*{{BAGL_ICON, 0x00, 30, 98, 50, 50, 0, 0, BAGL_FILL, 0, COLOR_BG_1, 0, 0},
     &ui_blue_lock_gif,
     0,
     0,
     0,
     NULL,
     NULL,
     NULL},
    {{BAGL_LABELINE, 0x31, 100, 117, 320, 30, 0, 0, BAGL_FILL, 0x000000,
      COLOR_BG_1, BAGL_FONT_OPEN_SANS_REGULAR_10_13PX, 0},
     tmp_string,
     0,
     0,
     0,
     NULL,
     NULL,
     NULL},*/
    {{BAGL_LABELINE, 0x00, 100, 138, 320, 30, 0, 0, BAGL_FILL, 0x999999,
      COLOR_BG_1, BAGL_FONT_OPEN_SANS_REGULAR_8_11PX, 0},
     "Confirm registration ?",
     0,
     0,
     0,
     NULL,
     NULL,
     NULL},

    /*{{BAGL_LABELINE, 0x00, 30, 196, 100, 30, 0, 0, BAGL_FILL, 0x000000,
      COLOR_BG_1, BAGL_FONT_OPEN_SANS_SEMIBOLD_8_11PX, 0},
     "SERVICE",
     0,
     0,
     0,
     NULL,
     NULL,
     NULL},*/
    // x-18 when ...
    /*{{BAGL_LABELINE, 0x01, 130, 196, 160, 30, 0, 0, BAGL_FILL, 0x000000,
      COLOR_BG_1,
      BAGL_FONT_OPEN_SANS_REGULAR_10_13PX | BAGL_FONT_ALIGNMENT_RIGHT, 0},
     verifyName,
     0,
     0,
     0,
     NULL,
     NULL,
     NULL},*/
    /*{{BAGL_LABELINE, 0x11, 284, 196, 6, 16, 0, 0, BAGL_FILL, 0x999999,
      COLOR_BG_1, BAGL_FONT_SYMBOLS_0 | BAGL_FONT_ALIGNMENT_RIGHT, 0},
     BAGL_FONT_SYMBOLS_0_MINIRIGHT,
     0,
     0,
     0,
     NULL,
     NULL,
     NULL},*/
    /*{{BAGL_NONE | BAGL_FLAG_TOUCHABLE, 0x00, 0, 168, 320, 48, 0, 9, BAGL_FILL,
      0xFFFFFF, 0x000000, 0, 0},
     NULL,
     0,
     0xEEEEEE,
     0x000000,
     ui_transaction_blue_service_details,
     ui_menu_item_out_over,
     ui_menu_item_out_over},*/
    /*{{BAGL_RECTANGLE, 0x11, 0, 168, 5, 48, 0, 0, BAGL_FILL, COLOR_BG_1,
      COLOR_BG_1, 0, 0},
     NULL,
     0,
     0x41CCB4,
     0,
     NULL,
     NULL,
     NULL},

    {{BAGL_RECTANGLE, 0x00, 30, 216, 260, 1, 1, 0, 0, 0xEEEEEE, COLOR_BG_1, 0,
      0},
     NULL,
     0,
     0,
     0,
     NULL,
     NULL,
     NULL},

    {{BAGL_LABELINE, 0x00, 30, 245, 100, 30, 0, 0, BAGL_FILL, 0x000000,
      COLOR_BG_1, BAGL_FONT_OPEN_SANS_SEMIBOLD_8_11PX, 0},
     "IDENTIFIER",
     0,
     0,
     0,
     NULL,
     NULL,
     NULL},*/
    // x-18 when ...
    /*{{BAGL_LABELINE, 0x02, 130, 245, 160, 30, 0, 0, BAGL_FILL, 0x000000,
      COLOR_BG_1,
      BAGL_FONT_OPEN_SANS_REGULAR_10_13PX | BAGL_FONT_ALIGNMENT_RIGHT, 0},
     verifyHash,
     0,
     0,
     0,
     NULL,
     NULL,
     NULL},*/
    /*{{BAGL_LABELINE, 0x12, 284, 245, 6, 16, 0, 0, BAGL_FILL, 0x999999,
      COLOR_BG_1, BAGL_FONT_SYMBOLS_0 | BAGL_FONT_ALIGNMENT_RIGHT, 0},
     BAGL_FONT_SYMBOLS_0_MINIRIGHT,
     0,
     0,
     0,
     NULL,
     NULL,
     NULL},*/
    /*{{BAGL_NONE | BAGL_FLAG_TOUCHABLE, 0x00, 0, 217, 320, 48, 0, 9, BAGL_FILL,
      0xFFFFFF, 0x000000, 0, 0},
     NULL,
     0,
     0xEEEEEE,
     0x000000,
     ui_transaction_blue_identifier_details,
     ui_menu_item_out_over,
     ui_menu_item_out_over},
    {{BAGL_RECTANGLE, 0x12, 0, 217, 5, 48, 0, 0, BAGL_FILL, COLOR_BG_1,
      COLOR_BG_1, 0, 0},
     NULL,
     0,
     0x41CCB4,
     0,
     NULL,
     NULL,
     NULL},*/

    {{BAGL_RECTANGLE | BAGL_FLAG_TOUCHABLE, 0x00, 40, 414, 115, 36, 0, 18,
      BAGL_FILL, 0xCCCCCC, COLOR_BG_1,
      BAGL_FONT_OPEN_SANS_REGULAR_11_14PX | BAGL_FONT_ALIGNMENT_CENTER |
          BAGL_FONT_ALIGNMENT_MIDDLE,
      0},
     "REJECT",
     0,
     0xB7B7B7,
     COLOR_BG_1,
     hello_register_callback_cancel_blue,
     NULL,
     NULL},
    {{BAGL_RECTANGLE | BAGL_FLAG_TOUCHABLE, 0x00, 165, 414, 115, 36, 0, 18,
      BAGL_FILL, 0x41ccb4, COLOR_BG_1,
      BAGL_FONT_OPEN_SANS_REGULAR_11_14PX | BAGL_FONT_ALIGNMENT_CENTER |
          BAGL_FONT_ALIGNMENT_MIDDLE,
      0},
     "CONFIRM",
     0,
     0x3ab7a2,
     COLOR_BG_1,
     hello_register_callback_confirm_blue,
     NULL,
     NULL},
};

const bagl_element_t ui_confirm_login_blue[] = {
    {{BAGL_RECTANGLE, 0x00, 0, 68, 320, 413, 0, 0, BAGL_FILL, COLOR_BG_1,
      0x000000, 0, 0},
     NULL,
     0,
     0,
     0,
     NULL,
     NULL,
     NULL},

    // erase screen (only under the status bar)
    {{BAGL_RECTANGLE, 0x00, 0, 20, 320, 48, 0, 0, BAGL_FILL, COLOR_APP,
      COLOR_APP, 0, 0},
     NULL,
     0,
     0,
     0,
     NULL,
     NULL,
     NULL},

    /// TOP STATUS BAR
    {{BAGL_LABELINE, 0x30, 0, 45, 320, 30, 0, 0, BAGL_FILL, 0xFFFFFF, COLOR_APP,
      BAGL_FONT_OPEN_SANS_SEMIBOLD_10_13PX | BAGL_FONT_ALIGNMENT_CENTER, 0},
     "WINDOWS HELLO",
     0,
     0,
     0,
     NULL,
     NULL,
     NULL},

    // BADGE_LOCK_BLUE.GIF
    /*{{BAGL_ICON, 0x00, 30, 98, 50, 50, 0, 0, BAGL_FILL, 0, COLOR_BG_1, 0, 0},
     &ui_blue_lock_gif,
     0,
     0,
     0,
     NULL,
     NULL,
     NULL},
    {{BAGL_LABELINE, 0x31, 100, 117, 320, 30, 0, 0, BAGL_FILL, 0x000000,
      COLOR_BG_1, BAGL_FONT_OPEN_SANS_REGULAR_10_13PX, 0},
     tmp_string,
     0,
     0,
     0,
     NULL,
     NULL,
     NULL},*/
    {{BAGL_LABELINE, 0x00, 100, 138, 320, 30, 0, 0, BAGL_FILL, 0x999999,
      COLOR_BG_1, BAGL_FONT_OPEN_SANS_REGULAR_8_11PX, 0},
     "Confirm login ?",
     0,
     0,
     0,
     NULL,
     NULL,
     NULL},

    /*{{BAGL_LABELINE, 0x00, 30, 196, 100, 30, 0, 0, BAGL_FILL, 0x000000,
      COLOR_BG_1, BAGL_FONT_OPEN_SANS_SEMIBOLD_8_11PX, 0},
     "SERVICE",
     0,
     0,
     0,
     NULL,
     NULL,
     NULL},*/
    // x-18 when ...
    /*{{BAGL_LABELINE, 0x01, 130, 196, 160, 30, 0, 0, BAGL_FILL, 0x000000,
      COLOR_BG_1,
      BAGL_FONT_OPEN_SANS_REGULAR_10_13PX | BAGL_FONT_ALIGNMENT_RIGHT, 0},
     verifyName,
     0,
     0,
     0,
     NULL,
     NULL,
     NULL},*/
    /*{{BAGL_LABELINE, 0x11, 284, 196, 6, 16, 0, 0, BAGL_FILL, 0x999999,
      COLOR_BG_1, BAGL_FONT_SYMBOLS_0 | BAGL_FONT_ALIGNMENT_RIGHT, 0},
     BAGL_FONT_SYMBOLS_0_MINIRIGHT,
     0,
     0,
     0,
     NULL,
     NULL,
     NULL},*/
    /*{{BAGL_NONE | BAGL_FLAG_TOUCHABLE, 0x00, 0, 168, 320, 48, 0, 9, BAGL_FILL,
      0xFFFFFF, 0x000000, 0, 0},
     NULL,
     0,
     0xEEEEEE,
     0x000000,
     ui_transaction_blue_service_details,
     ui_menu_item_out_over,
     ui_menu_item_out_over},*/
    /*{{BAGL_RECTANGLE, 0x11, 0, 168, 5, 48, 0, 0, BAGL_FILL, COLOR_BG_1,
      COLOR_BG_1, 0, 0},
     NULL,
     0,
     0x41CCB4,
     0,
     NULL,
     NULL,
     NULL},

    {{BAGL_RECTANGLE, 0x00, 30, 216, 260, 1, 1, 0, 0, 0xEEEEEE, COLOR_BG_1, 0,
      0},
     NULL,
     0,
     0,
     0,
     NULL,
     NULL,
     NULL},

    {{BAGL_LABELINE, 0x00, 30, 245, 100, 30, 0, 0, BAGL_FILL, 0x000000,
      COLOR_BG_1, BAGL_FONT_OPEN_SANS_SEMIBOLD_8_11PX, 0},
     "IDENTIFIER",
     0,
     0,
     0,
     NULL,
     NULL,
     NULL},*/
    // x-18 when ...
    /*{{BAGL_LABELINE, 0x02, 130, 245, 160, 30, 0, 0, BAGL_FILL, 0x000000,
      COLOR_BG_1,
      BAGL_FONT_OPEN_SANS_REGULAR_10_13PX | BAGL_FONT_ALIGNMENT_RIGHT, 0},
     verifyHash,
     0,
     0,
     0,
     NULL,
     NULL,
     NULL},*/
    /*{{BAGL_LABELINE, 0x12, 284, 245, 6, 16, 0, 0, BAGL_FILL, 0x999999,
      COLOR_BG_1, BAGL_FONT_SYMBOLS_0 | BAGL_FONT_ALIGNMENT_RIGHT, 0},
     BAGL_FONT_SYMBOLS_0_MINIRIGHT,
     0,
     0,
     0,
     NULL,
     NULL,
     NULL},*/
    /*{{BAGL_NONE | BAGL_FLAG_TOUCHABLE, 0x00, 0, 217, 320, 48, 0, 9, BAGL_FILL,
      0xFFFFFF, 0x000000, 0, 0},
     NULL,
     0,
     0xEEEEEE,
     0x000000,
     ui_transaction_blue_identifier_details,
     ui_menu_item_out_over,
     ui_menu_item_out_over},
    {{BAGL_RECTANGLE, 0x12, 0, 217, 5, 48, 0, 0, BAGL_FILL, COLOR_BG_1,
      COLOR_BG_1, 0, 0},
     NULL,
     0,
     0x41CCB4,
     0,
     NULL,
     NULL,
     NULL},*/

    {{BAGL_RECTANGLE | BAGL_FLAG_TOUCHABLE, 0x00, 40, 414, 115, 36, 0, 18,
      BAGL_FILL, 0xCCCCCC, COLOR_BG_1,
      BAGL_FONT_OPEN_SANS_REGULAR_11_14PX | BAGL_FONT_ALIGNMENT_CENTER |
          BAGL_FONT_ALIGNMENT_MIDDLE,
      0},
     "REJECT",
     0,
     0xB7B7B7,
     COLOR_BG_1,
     hello_login_callback_cancel_blue,
     NULL,
     NULL},
    {{BAGL_RECTANGLE | BAGL_FLAG_TOUCHABLE, 0x00, 165, 414, 115, 36, 0, 18,
      BAGL_FILL, 0x41ccb4, COLOR_BG_1,
      BAGL_FONT_OPEN_SANS_REGULAR_11_14PX | BAGL_FONT_ALIGNMENT_CENTER |
          BAGL_FONT_ALIGNMENT_MIDDLE,
      0},
     "CONFIRM",
     0,
     0x3ab7a2,
     COLOR_BG_1,
     hello_login_callback_confirm_blue,
     NULL,
     NULL},
};

unsigned int hello_login_callback_cancel_blue(void){
	hello_login_cancel();
	return 0;
}

unsigned int hello_login_callback_confirm_blue(void){
	hello_login_confirm();
	return 0;
}


unsigned int hello_register_callback_cancel_blue(void){
	hello_register_cancel();
  return 0;
}

unsigned int hello_register_callback_confirm_blue(void){
	hello_register_confirm();
  return 0;
}

void ui_idle_init(void) {
  ux_step = 0;
  ux_step_count = 2;
  UX_SET_STATUS_BAR_COLOR(0xFFFFFF, COLOR_APP);
  UX_DISPLAY(ui_idle_mainmenu_blue, NULL);

  // UX_DISPLAY(ui_idle_mainmenu_list_blue, NULL);
  // UX_DISPLAY(ui_idle_mainmenu_list_blue, ui_idle_mainmenu_list_blue_prepro);
  
  // setup the first screen changing
  UX_CALLBACK_SET_INTERVAL(1000);
}

unsigned int ui_confirm_registration_blue_button(unsigned int button_mask,
                                        unsigned int button_mask_counter) {
    return 0;
}

unsigned int ui_confirm_login_blue_button(unsigned int button_mask,
                                        unsigned int button_mask_counter) {
    return 0;
}


void ui_confirm_registration_init(void){
	UX_DISPLAY(ui_confirm_registration_blue, NULL);
}

void ui_confirm_login_init(void){
	UX_DISPLAY(ui_confirm_login_blue, NULL);
}

unsigned int ui_settings_back_callback(const bagl_element_t* e) {
  // go back to idle
  ui_idle_init();
  return 0;
}

const bagl_element_t * ui_settings_blue_toggle_confirm_login_blue(const bagl_element_t * e) {
  // swap setting and request redraw of settings elements
  uint8_t setting = N_storage.dont_confirm_login?0:1;
  /*uint8_t setting;
  if (N_storage.dont_confirm_login == 0){
  	setting = 1;
  }
  else{
  	setting = 0;
  }*/

  nvm_write(&N_storage.dont_confirm_login, (void*)&setting, sizeof(uint8_t));

  // only refresh settings mutable drawn elements
  #ifdef DYNAMIC_LOCK
    UX_REDISPLAY_IDX(12);
  #else
    UX_REDISPLAY_IDX(9);
  #endif

  // won't redisplay the bagl_none
  return 0;
}

#ifdef DYNAMIC_LOCK
const bagl_element_t * ui_settings_blue_toggle_dlock_blue(const bagl_element_t * e) {
  // swap setting and request redraw of settings elements
  uint8_t setting = N_storage.dynamic_lock?0:1;
  nvm_write(&N_storage.dynamic_lock, (void*)&setting, sizeof(uint8_t));

  // only refresh settings mutable drawn elements
  UX_REDISPLAY_IDX(12);

  // won't redisplay the bagl_none
  return 0;
}
#endif

const bagl_element_t* ui_settings_out_over(const bagl_element_t* e) {
  return NULL;
}

const bagl_element_t * ui_settings_blue_prepro(const bagl_element_t * e) {
  // none elements are skipped
  if ((e->component.type&(~BAGL_FLAG_TOUCHABLE)) == BAGL_NONE) {
    return 0;
  }
  // swap icon buffer to be displayed depending on if corresponding setting is enabled or not.
  if (e->component.userid) {
    os_memmove(&tmp_element, e, sizeof(bagl_element_t));
    switch(e->component.userid) {
      case 0x01:
        // swap icon content
        if (N_storage.dont_confirm_login) {
          tmp_element.text = &C_icon_toggle_set;
        }
        else {
          tmp_element.text = &C_icon_toggle_reset;
        }
        break;
      #ifdef DYNAMIC_LOCK
      case 0x02:
        // swap icon content
        if (N_storage.dynamic_lock) {
          tmp_element.text = &C_icon_toggle_set;
        }
        else {
          tmp_element.text = &C_icon_toggle_reset;
        }
        break;
      #endif
    }
    return &tmp_element;
  }
  return 1;
}

unsigned int ui_idle_mainmenu_blue_button(unsigned int button_mask, unsigned int button_mask_counter) {
  return 0;
}

unsigned int ui_idle_mainmenu_list_blue_button(unsigned int button_mask, unsigned int button_mask_counter) {
  return 0;
}

unsigned int ui_settings_blue_button(unsigned int button_mask, unsigned int button_mask_counter) {
  return 0;  
}

unsigned int io_seproxyhal_touch_settings(const bagl_element_t *e) {
  UX_DISPLAY(ui_settings_blue, ui_settings_blue_prepro);
  return 0; // do not redraw button, screen has switched
}

unsigned int io_seproxyhal_touch_exit(const bagl_element_t *e) {
    // Go back to the dashboard
    os_sched_exit(0);
    return 0; // do not redraw the widget
}

#endif 
