#include <stdlib.h>

// The converter returns malloc-owned UTF-8; free it in the same native library.
__attribute__((visibility("default"))) void RodyFrench_Free(void *value) { free(value); }
