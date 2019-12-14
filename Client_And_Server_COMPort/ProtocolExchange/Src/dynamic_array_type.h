/*
 * dynamic_array_type.h
 *
 * Created: 08.12.2019 17:49:24
 *  Author: Developer
 */ 

#include "const.h"

typedef struct {
	byte* array;
	int used;
	int size;
} DynamicArray;