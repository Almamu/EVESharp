'''
opcode module - potentially shared between dis and other modules which
operate on bytecodes (e.g. peephole optimizers).
'''

cmp_op = ('<', '<=', '==', '!=', '>', '>=', 'in', 'not in', 'is',
        'is not', 'exception match', 'BAD')

hasconst = []
hasname = []
hasjrel = []
hasjabs = []
haslocal = []
hascompare = []
hasfree = []
hasArgumentExtended = []
PJIF = PJIT = JA = JF = 0

opmap = {}
opname = [''] * 256
for op in range(256): opname[op] = '<%r>' % (op,)
del op

def def_op(name, op):
    opname[op] = name
    opmap[name] = op
    globals().update({name: op}) 

def name_op(name, op):
    def_op(name, op)
    hasname.append(op)

def jrel_op(name, op):
    def_op(name, op)
    hasjrel.append(op)

def jabs_op(name, op):
    def_op(name, op)
    hasjabs.append(op)

def def_extArg(name, op):
    def_op(name, op)
    hasArgumentExtended.append(op)
    
def updateGlobal():
    globals().update({'PJIF': opmap['JUMP_IF_FALSE']})
    globals().update({'PJIT': opmap['JUMP_IF_TRUE']})
    globals().update({'JA': opmap['JUMP_ABSOLUTE']})
    globals().update({'JF': opmap['JUMP_FORWARD']})
    update = {}
    for(k,v) in opmap.items():
        update[k.replace('+','_')] = v
    globals().update(update)
    globals().update({'JUMP_OPs': map(lambda op: opname[op], hasjrel + hasjabs)})
    
# Instruction opcodes for compiled code
# Blank lines correspond to available opcodes

def_op('STOP_CODE', 0) 	# 0
def_op('POP_TOP', 1)	# 15
def_op('ROT_TWO', 2)	# 59
def_op('ROT_THREE', 3)	# 60
def_op('DUP_TOP', 4)	# 13
def_op('ROT_FOUR', 5)	# 49

def_op('NOP', 9)		# 53
def_op('UNARY_POSITIVE', 10) # 48
def_op('UNARY_NEGATIVE', 11) # 54
def_op('UNARY_NOT', 12)		# 38
def_op('UNARY_CONVERT', 13)	# 25

def_op('UNARY_INVERT', 15)	# 34

def_extArg('LIST_APPEND', 18)	# 68
def_op('BINARY_POWER', 19)	# 28
def_op('BINARY_MULTIPLY', 20) # 36
def_op('BINARY_DIVIDE', 21) # 12
def_op('BINARY_MODULO', 22) # 41
def_op('BINARY_ADD', 23)	# 52
def_op('BINARY_SUBTRACT', 24) # 55
def_op('BINARY_SUBSCR', 25) # 4
def_op('BINARY_FLOOR_DIVIDE', 26) # 43
def_op('BINARY_TRUE_DIVIDE', 27) # 5
def_op('INPLACE_FLOOR_DIVIDE', 28) # 32
def_op('INPLACE_TRUE_DIVIDE', 29) # 30
def_op('SLICE+0', 30)		# 16
def_op('SLICE+1', 31)		# 17
def_op('SLICE+2', 32)		# 18
def_op('SLICE+3', 33)		# 19

def_op('STORE_SLICE+0', 40) # 61
def_op('STORE_SLICE+1', 41) # 62
def_op('STORE_SLICE+2', 42) # 63
def_op('STORE_SLICE+3', 43) # 64

def_op('DELETE_SLICE+0', 50) # 44
def_op('DELETE_SLICE+1', 51) # 45
def_op('DELETE_SLICE+2', 52) # 46
def_op('DELETE_SLICE+3', 53) # 47

def_op('INPLACE_ADD', 55)	# 6
def_op('INPLACE_SUBTRACT', 56) # 29
def_op('INPLACE_MULTIPLY', 57) # 8
def_op('INPLACE_DIVIDE', 58) # 27
def_op('INPLACE_MODULO', 59) # 3
def_op('STORE_SUBSCR', 60)	# 31
def_op('DELETE_SUBSCR', 61) # 69
def_op('BINARY_LSHIFT', 62) # 7
def_op('BINARY_RSHIFT', 63) # 22
def_op('BINARY_AND', 64)	# 50
def_op('BINARY_XOR', 65)	# 21
def_op('BINARY_OR', 66)		# 2
def_op('INPLACE_POWER', 67) # 57
def_op('GET_ITER', 68)		# 39

def_op('PRINT_EXPR', 70)	# 20
def_op('PRINT_ITEM', 71)	# 9
def_op('PRINT_NEWLINE', 72) # 14
def_op('PRINT_ITEM_TO', 73) # 33
def_op('PRINT_NEWLINE_TO', 74) # 35
def_op('INPLACE_LSHIFT', 75) # 11
def_op('INPLACE_RSHIFT', 76) # 58
def_op('INPLACE_AND', 77)	# 24
def_op('INPLACE_XOR', 78)	# 23
def_op('INPLACE_OR', 79)	# 10
def_op('BREAK_LOOP', 80)	# 40
def_op('WITH_CLEANUP', 81)	# 37
def_op('LOAD_LOCALS', 82)	# 51
def_op('RETURN_VALUE', 83)	# 66
def_op('IMPORT_STAR', 84)	# 56
def_op('EXEC_STMT', 85)		# 65
def_op('YIELD_VALUE', 86)	# 26
def_op('POP_BLOCK', 87)		# 1
def_op('END_FINALLY', 88)	# 67
def_op('BUILD_CLASS', 89)	# 42

HAVE_ARGUMENT = 90         # 70    # Opcodes from here have an argument:

name_op('STORE_NAME', 90)   # 95    # Index in name list
name_op('DELETE_NAME', 91)  # 94   # ""
def_op('UNPACK_SEQUENCE', 92) # 93  # Number of tuple items
jrel_op('FOR_ITER', 93)		# 81

name_op('STORE_ATTR', 95) 	# 84      # Index in name list
name_op('DELETE_ATTR', 96) 	# 87     # ""
name_op('STORE_GLOBAL', 97) # 105     # ""
name_op('DELETE_GLOBAL', 98) # 98   # ""
def_op('DUP_TOPX', 99) 		# 104         # number of items to duplicate
def_op('LOAD_CONST', 100) 	# 72      # Index in const list
hasconst.append(100)		# 72
name_op('LOAD_NAME', 101) 	# 79      # Index in name list
def_op('BUILD_TUPLE', 102) 	# 80     # Number of tuple items
def_op('BUILD_LIST', 103) 	# 107      # Number of list items
def_op('BUILD_MAP', 104) 	# 78       # Always zero for now
name_op('LOAD_ATTR', 105) 	# 86      # Index in name list
def_op('COMPARE_OP', 106) 	# 101      # Comparison operator
hascompare.append(106)		# 101
name_op('IMPORT_NAME', 107) # 88    # Index in name list
name_op('IMPORT_FROM', 108) # 89    # Index in name list

jrel_op('JUMP_FORWARD', 110) # 73   # Number of bytes to skip
jabs_op('JUMP_IF_FALSE', 111) # 83  # ""
jabs_op('JUMP_IF_TRUE', 112) # 90   # ""
jabs_op('JUMP_ABSOLUTE', 113) # 103  # Target byte offset from beginning of code

name_op('LOAD_GLOBAL', 116) # 70    # Index in name list

jabs_op('CONTINUE_LOOP', 119) # 96  # Target address
jrel_op('SETUP_LOOP', 120) 	# 74     # Distance to target address
jrel_op('SETUP_EXCEPT', 121) # 75   # ""
jrel_op('SETUP_FINALLY', 122) # 106  # ""

def_op('LOAD_FAST', 124) 	# 92       # Local variable number
haslocal.append(124)		# 92
def_op('STORE_FAST', 125) 	# 82      # Local variable number
haslocal.append(125)		# 82
def_op('DELETE_FAST', 126)  # 71     # Local variable number
haslocal.append(126)		# 71

def_op('RAISE_VARARGS', 130) # 91   # Number of raise arguments (1, 2, or 3)
def_op('CALL_FUNCTION', 131) # 102   # #args + (#kwargs << 8)
def_op('MAKE_FUNCTION', 132) # 76   # Number of args with default values
def_op('BUILD_SLICE', 133)  # 77    # Number of items
def_op('MAKE_CLOSURE', 134) # 85
def_op('LOAD_CLOSURE', 135) # 97
hasfree.append(135)			# 97
def_op('LOAD_DEREF', 136)	# 99
hasfree.append(136)			# 99
def_op('STORE_DEREF', 137)	# 100
hasfree.append(137)			# 100

def_op('CALL_FUNCTION_VAR', 140) # 111    # #args + (#kwargs << 8)
def_op('CALL_FUNCTION_KW', 141) # 112     # #args + (#kwargs << 8)
def_op('CALL_FUNCTION_VAR_KW', 142) # 113 # #args + (#kwargs << 8)
def_op('EXTENDED_ARG', 143) # 114
EXTENDED_ARG = 143			# 114

updateGlobal()
del def_op, name_op, jrel_op, jabs_op
