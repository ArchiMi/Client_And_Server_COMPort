#include "Src/const.h"
#include "Src/utils.h"
#include <avr/io.h>
#include <util/delay.h>
#include <avr/pgmspace.h>
#include <avr/wdt.h>
#include <avr/interrupt.h>
#include <stdbool.h>


void USART_Init() {
	sei();
	
	//BEGIN Test	
	wdt_disable();
	
	//bool WD_RST = MCUSR & 0x08;
	//bool BO_RST = MCUSR & 0x04;
	//bool EXT_RST = MCUSR & 0x02;
	//bool PON_RST = MCUSR & 0x01;
	MCUSR = MCUSR & 0xF0;
	//END Test
		
	wdt_enable(WDTO_8S);
	
	WDTCSR = 1<<WDIE;
	
	//UCSR0A |= (1<<U2X0); //�������� �������
	UCSR0B = (1<<RXEN0) | (1<<TXEN0); //Trust transmit and receive from USART - T/R ENable = True
	UCSR0C = (0<<UMSEL01) | (0<<UMSEL00) | (1<<USBS0) | (1<<UCSZ00) | (1<<UCSZ01) | (1 << UCSZ00);
	
	UBRR0L = BAUD_PRESCALE;
	UBRR0H = BAUD_PRESCALE >> 8;

	// �� ����� 20 ����������� �������� ������������
	_delay_ms(20);
}

byte USART_Receive(void) {	
	while( !(UCSR0A & (1<<RXC0)) );
	return UDR0;	
}

void USART_Receive_Str(byte *calledstring, DynamicArray* a) {
	char ch;
	
	int i = 0;	
	while(1) {				
		// Check error from receive 
		if (i == FRAME_SIZE) {
			return;
		}		
	
		// Get input char	
		ch = USART_Receive();
		
		// Check first char equal COLON
		if (ch != CHR_COLON && i == 0) {
			continue;
		} else {
			if (ch == CHR_LINE_FEED && i > 0) {
				int previous_index = i - 1;
 				if (calledstring[previous_index] == CHR_CARRET_RETURN) {
					calledstring[i] = CHR_LINE_FEED;
					insertArray(a, CHR_LINE_FEED);
					return;
				} else {
					calledstring[i] = ch;
					insertArray(a, ch);
				}
			} else {
				calledstring[i] = ch;
				insertArray(a, ch);
			}	
		}
		
		i++;
	}	
}

void USART_Send(byte data) {
	while( !(UCSR0A & ( 1 << UDRE0 )) );
	UDR0 = data;
}

void USART_Transmit_Str(DynamicArray* a) {
	// Chars array
	/*
	for (int i = 0; i <= FRAME_SIZE; i++) {
		if (calledstring[i] != 0)
			// Send char
			USART_Send(calledstring[i]);
		else 
			break;		
	}
	*/
	
	//Array
	int count = a->size;
	for (int i = 0; i < count; i++) {		
		if (a->array[i] != 0)
			// Send char
			USART_Send(a->array[i]);
			//USART_Send(55);
		else
			break;
	}
	
	// Info Blink
	blink();
}

void Clean_Data(byte *input_data) {
	for (uint8_t i = 0; i < FRAME_SIZE; i++) {
		input_data[i] = 0;
	}
}

// Watch dog
ISR(WDT_vect) {
	wdt_reset();

	//USART_Init();
	
	blink_WD();
}

int main(void) {
	DDRB = 0xFF; //PORTB as Output
	
	byte input[FRAME_SIZE] = { 0 };
	
	USART_Init();       
	
	while(1) {
		// Create array
		DynamicArray inputArray;
		initArray(&inputArray, 32);
		
		// Get request
		USART_Receive_Str(input, &inputArray);
		
		// Get CRC8 byte index
		byte index_crc8 = getCRC8Index(input, &inputArray, FRAME_SIZE);
			
		// Add CRC8 byte
		byte crc8_code = crc8dy(&inputArray, index_crc8);
		//input[index_crc8] = crc8_code;
		
		if (inputArray.array[index_crc8] == crc8_code) {
						
			//Clear request data
			freeArray(&inputArray);	
			initArray(&inputArray, 50);
			
			insertArray(&inputArray, CHR_COLON);
			insertArray(&inputArray, 1);
			insertArray(&inputArray, 2);
			insertArray(&inputArray, 3);
			insertArray(&inputArray, crc8_code);
			insertArray(&inputArray, CHR_COLON);
			insertArray(&inputArray, CHR_CARRET_RETURN);
			insertArray(&inputArray, CHR_LINE_FEED);
			
			// Send response ( ECHO )
			USART_Transmit_Str(&inputArray);			
		} else /* ERROR */ {
						
			freeArray(&inputArray);	
			initArray(&inputArray, 32);
			
			insertArray(&inputArray, CHR_COLON);
			insertArray(&inputArray, crc8_code);			
			insertArray(&inputArray, CHR_COLON);
			insertArray(&inputArray, CHR_CARRET_RETURN);
			insertArray(&inputArray, CHR_LINE_FEED);
			
			// Send response
			USART_Transmit_Str(&inputArray);
		}
		
		// Clear array
		Clean_Data(input);
		
		// Free Array
		freeArray(&inputArray);	
		
		// Info Blink
		//blink();
	}
	
	return 0;
}