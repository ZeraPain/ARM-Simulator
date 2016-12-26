@@testfile	

.text
	mov r0, #4
main:
	mov r1, #1
	mov r2, #3
	mov r3, #3

L1:	add r2, r2, r1
	sub r3, r3, r1
	stm sp!, {r1-r4}
	cmp r2, #5
	beq main
	b L1

.data	
X:	.word	0x47114711
Y:	.word	0x0f0f0f0f
Z:	.word	0xaabbccdd