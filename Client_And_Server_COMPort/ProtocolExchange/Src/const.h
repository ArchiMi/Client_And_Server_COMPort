/*
 * const.h
 *
 * Created: 22.09.2019 11:44:31
 *  Author: Developer
 */ 

typedef unsigned char byte;

#define F_CPU 16000000UL
#define NULL ((void*)0)

// In table 'Examples of UBRRn Settings' for ATmega328P U2X=1 "7 - 256000 Baud" or 207 - 9600
//#define BAUD_PRESCALE 207 //207 - 9600 U2X=1
#define BAUD_PRESCALE 3 //3 - 256000 U2X=0

#define FRAME_SIZE 32
#define CHR_CARRET_RETURN 13
#define CHR_LINE_FEED 10
#define CHR_COLON 58