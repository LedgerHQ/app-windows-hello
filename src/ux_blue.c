#if defined (TARGET_BLUE)

void ui_idle_init(void) {
  ux_step = 0;
  ux_step_count = 2;
  UX_SET_STATUS_BAR_COLOR(0xFFFFFF, COLOR_APP);
  UX_DISPLAY(ui_idle_mainmenu_blue, NULL);
  // setup the first screen changing
  UX_CALLBACK_SET_INTERVAL(1000);
}

#endif 
