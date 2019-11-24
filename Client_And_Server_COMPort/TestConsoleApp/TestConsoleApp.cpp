// TestConsoleApp.cpp : Этот файл содержит функцию "main". Здесь начинается и заканчивается выполнение программы.
//

#include <iostream>

typedef unsigned char byte;

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

void Testfunc() {
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
}

int main()
{
    //std::cout << "Hello World!\n";

	Testfunc();

}

// Запуск программы: CTRL+F5 или меню "Отладка" > "Запуск без отладки"
// Отладка программы: F5 или меню "Отладка" > "Запустить отладку"

// Советы по началу работы 
//   1. В окне обозревателя решений можно добавлять файлы и управлять ими.
//   2. В окне Team Explorer можно подключиться к системе управления версиями.
//   3. В окне "Выходные данные" можно просматривать выходные данные сборки и другие сообщения.
//   4. В окне "Список ошибок" можно просматривать ошибки.
//   5. Последовательно выберите пункты меню "Проект" > "Добавить новый элемент", чтобы создать файлы кода, или "Проект" > "Добавить существующий элемент", чтобы добавить в проект существующие файлы кода.
//   6. Чтобы снова открыть этот проект позже, выберите пункты меню "Файл" > "Открыть" > "Проект" и выберите SLN-файл.
