#include "ux_common.h"

unsigned int ux_step;
unsigned int ux_step_count;
unsigned char device_key[32];
unsigned char auth_key[32];


/*typedef struct internalStorage_t {
// #define STORAGE_MAGIC 0xDEAD1337
//     uint32_t magic;
    uint32_t dont_confirm_login;
    uint32_t dynamic_lock;
} internalStorage_t;*/

WIDE internalStorage_t N_storage_real;
#define N_storage (*(WIDE internalStorage_t *)PIC(&N_storage_real)) 
