#  Copyright (c) 2016-2017 by Rocky Bernstein
"""
Python 2.4 bytecode massaging.

This massages tokenized 2.7 bytecode to make it more amenable for
grammar parsing.
"""

import uncompyle6.scanners.scanner25 as scan

# bytecode verification, verify(), uses JUMP_OPs from here
from xdis.opcodes import opcode_24
JUMP_OPS = opcode_24.JUMP_OPS

# We base this off of 2.5 instead of the other way around
# because we cleaned things up this way.
# The history is that 2.7 support is the cleanest,
# then from that we got 2.6 and so on.
class Scanner24(scan.Scanner25):
    def __init__(self, show_asm=False):
        scan.Scanner25.__init__(self, show_asm)
        # These are the only differences in initialization between
        # 2.4, 2.5 and 2.6
        self.opc = opcode_24
        self.opname = opcode_24.opname
        self.version = 2.4
        self.genexpr_name = '<generator expression>'
        return
