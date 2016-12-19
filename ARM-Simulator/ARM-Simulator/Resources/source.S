@@testfile
	mov r0, #4

main:
	mov r1, #1
	mov r2, #3
	mov r3, #3

L1:	add r2, r2, r1
	sub r3, r3, r1
	cmp r2, #5
	beq main
	b L1










































