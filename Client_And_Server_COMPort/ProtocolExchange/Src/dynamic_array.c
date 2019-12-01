/*
 * dynamic_array.c
 *
 * Created: 30.11.2019 17:10:24
 *  Author: Developer
 */

/*	
	USED:
 
	Array a;
	int i;
	 
	initArray(&a, 5);  // Initially 5 elements
	for (i = 0; i < 100; i++) {
		insertArray(&a, i);  // Automatically resizes as necessary
	}
	 
	int count = a.size;
	for (i = 0; i < count; i++) {
		a.array[i] = i;
	}
	 
	//a.array[9]  // Get and set byte to array element
	//a.used  // Number of elements
	freeArray(&a);
 */
 

#include "const.h"
#include <avr/pgmspace.h>

typedef struct {
	byte* array;
	int used;
	int size;
} Array;

void initArray(Array* a, size_t initialSize) {
	a->array = (byte*)malloc(initialSize * sizeof(byte));
	a->used = 0;
	a->size = initialSize;
}

void insertArray(Array* a, byte element_byte) {
	if (a->used == a->size) {
		a->size += 1;
		byte* newarr = (byte*)realloc(a->array, a->size * sizeof(byte));
		if (newarr == NULL) return; // Block don't selected - return
		a->array = newarr;
	}

	a->array[a->used++] = element_byte;
}

void freeArray(Array* a) {
	free(a->array);
	a->array = NULL;
	a->used = a->size = 0;
}
