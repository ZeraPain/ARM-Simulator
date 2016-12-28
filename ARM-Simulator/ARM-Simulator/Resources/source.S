@@testfile	

.text
xptr:
	.word X
	mov r0, #4
main:
	mov r1, #1
	mov r2, #3
	mov r3, #3
	ldr r7, main
	ldr r5, xptr
	ldr r6, [r5]
	sub r6, r6, #1
	str r6, [r5]

L1:	add r2, r2, r1
	sub r3, r3, r1
	stm sp!, {r1-r4}
	cmp r2, #5
	beq main
	b L1

yptr:
	.word Y
zptr:
	.word Z

.data	
X:	.word	0x47114711
Y:	.word	0x0f0f0f0f
Z:	.word	0xaabbccdd
