# (C) Copyright 2017, 2020 by Rocky Bernstein
"""
CPython 2.4 bytecode opcodes

This is a like Python 2.3's opcode.py with some additional classification
of stack usage, and opererand formatting functions.
"""

import xdis.opcodes.opcode_2x as opcode_2x
from xdis.opcodes.base import (
    def_op,
    extended_format_ATTR,
    extended_format_CALL_FUNCTION,
    extended_format_MAKE_FUNCTION_older,
    extended_format_RAISE_VARARGS_older,
    extended_format_RETURN_VALUE,
    init_opdata,
    finalize_opcodes,
    format_CALL_FUNCTION_pos_name_encoded,
    format_MAKE_FUNCTION_default_argc,
    format_RAISE_VARARGS_older,
    format_extended_arg,
    update_pj2
)

version = 2.4
python_implementation = "CPython"

l = locals()
init_opdata(l, opcode_2x, version)

# Bytecodes added since 2.3
#          OP NAME            OPCODE POP PUSH
#--------------------------------------------
def_op(l, 'NOP',                   9,  0,  0)
def_op(l, 'LIST_APPEND',          18,  2,  0)  # Calls list.append(TOS[-i], TOS).
                                               # Used to implement list comprehensions.
def_op(l, 'YIELD_VALUE',          86,  1,  1)

# FIXME remove (fix uncompyle6)
update_pj2(globals(), l)

finalize_opcodes(l)

opcode_arg_fmt = {
    "CALL_FUNCTION": format_CALL_FUNCTION_pos_name_encoded,
    "CALL_FUNCTION_KW": format_CALL_FUNCTION_pos_name_encoded,
    "CALL_FUNCTION_VAR_KW": format_CALL_FUNCTION_pos_name_encoded,
    "EXTENDED_ARG": format_extended_arg,
    "MAKE_FUNCTION": format_MAKE_FUNCTION_default_argc,
    "RAISE_VARARGS": format_RAISE_VARARGS_older,
}

opcode_extended_fmt = {
    "CALL_FUNCTION": extended_format_CALL_FUNCTION,
    "LOAD_ATTR": extended_format_ATTR,
    "MAKE_FUNCTION": extended_format_MAKE_FUNCTION_older,
    "RAISE_VARARGS": extended_format_RAISE_VARARGS_older,
    "RETURN_VALUE": extended_format_RETURN_VALUE,
    "STORE_ATTR": extended_format_ATTR,
}
