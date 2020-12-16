# (C) Copyright 2018-2019 by Rocky Bernstein
#
#  This program is free software; you can redistribute it and/or
#  modify it under the terms of the GNU General Public License
#  as published by the Free Software Foundation; either version 2
#  of the License, or (at your option) any later version.
#
#  This program is distributed in the hope that it will be useful,
#  but WITHOUT ANY WARRANTY; without even the implied warranty of
#  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
#  GNU General Public License for more details.
#
#  You should have received a copy of the GNU General Public License
#  along with this program; if not, write to the Free Software
#  Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

"""Facilitates importing opmaps for the a given Python version"""
import sys
from xdis import IS_PYPY
from xdis.magics import canonic_python_version

from xdis.opcodes import opcode_10 as opcode_10
from xdis.opcodes import opcode_11 as opcode_11
from xdis.opcodes import opcode_13 as opcode_13
from xdis.opcodes import opcode_14 as opcode_14
from xdis.opcodes import opcode_15 as opcode_15
from xdis.opcodes import opcode_16 as opcode_16
from xdis.opcodes import opcode_20 as opcode_20
from xdis.opcodes import opcode_21 as opcode_21
from xdis.opcodes import opcode_22 as opcode_22
from xdis.opcodes import opcode_23 as opcode_23
from xdis.opcodes import opcode_24 as opcode_24
from xdis.opcodes import opcode_25 as opcode_25
from xdis.opcodes import opcode_26 as opcode_26
from xdis.opcodes import opcode_27 as opcode_27
from xdis.opcodes import opcode_30 as opcode_30
from xdis.opcodes import opcode_31 as opcode_31
from xdis.opcodes import opcode_32 as opcode_32
from xdis.opcodes import opcode_33 as opcode_33
from xdis.opcodes import opcode_34 as opcode_34
from xdis.opcodes import opcode_35 as opcode_35
from xdis.opcodes import opcode_36 as opcode_36
from xdis.opcodes import opcode_37 as opcode_37
from xdis.opcodes import opcode_38 as opcode_38
from xdis.opcodes import opcode_39 as opcode_39

from xdis.opcodes import opcode_26pypy as opcode_26pypy
from xdis.opcodes import opcode_27pypy as opcode_27pypy
from xdis.opcodes import opcode_32pypy as opcode_32pypy
from xdis.opcodes import opcode_33pypy as opcode_33pypy
from xdis.opcodes import opcode_35pypy as opcode_35pypy
from xdis.opcodes import opcode_36pypy as opcode_36pypy

# FIXME
op_imports = {
    1.0     : opcode_10,
    '1.0'   : opcode_10,
    1.1     : opcode_11,
    '1.1'   : opcode_11,
    1.2     : opcode_11,
    '1.2'   : opcode_11,
    1.3     : opcode_13,
    '1.3'   : opcode_13,
    1.4     : opcode_14,
    '1.4'   : opcode_14,
    1.5     : opcode_15,
    '1.5'   : opcode_15,
    1.6     : opcode_16,
    '1.6'   : opcode_16,
    '2.0'   : opcode_20,
    2.0     : opcode_20,
    '2.1'   : opcode_21,
    2.1     : opcode_21,
    '2.2'   : opcode_22,
    2.2     : opcode_22,
    '2.3a0' : opcode_23,
    2.3     : opcode_23,
    '2.4b1' : opcode_24,
    2.4     : opcode_24,
    '2.5c2' : opcode_25,
    2.5     : opcode_25,
    '2.6a1' : opcode_26,
    2.6     : opcode_26,
    '2.7'   : opcode_27,
    2.7     : opcode_27,
    '2.7.18candidate1' : opcode_27,
    '3.0'   : opcode_30,
    3.0     : opcode_30,
    '3.0a5' : opcode_30,
    '3.1'   : opcode_31,
    '3.1a0+': opcode_31,
    3.1     : opcode_31,
    '3.2'   : opcode_32,
    '3.2a2' : opcode_32,
    3.2     : opcode_32,
    '3.3a4' : opcode_33,
    3.3     : opcode_33,
    '3.4'   : opcode_34,
    '3.4rc2': opcode_34,
    3.4     : opcode_34,
    '3.5'   : opcode_35,
    '3.5.1' : opcode_35,
    '3.5.2' : opcode_35,
    '3.5.3' : opcode_35,
    '3.5.4' : opcode_35,
    3.5     : opcode_35,
    '3.6rc1': opcode_36,
    '3.6rc1': opcode_36,
    3.6     : opcode_36,
    '3.7.0beta3': opcode_37,
    '3.7.0.beta3': opcode_37,
    '3.7.0' : opcode_37,
    3.7     : opcode_37,
    '3.8.0alpha0' : opcode_38,
    '3.8.0a0': opcode_38,
    '3.8.0a3+': opcode_38,
    '3.8.0alpha3': opcode_38,
    '3.8.0beta2': opcode_38,
    '3.8.0rc1+': opcode_38,
    '3.8.0candidate1': opcode_38,
    '3.8' : opcode_38,
    '3.9.0alpha1' : opcode_39,
    '3.9.0alpha2' : opcode_39,
    '3.9.0beta5' : opcode_39,
    '3.9' : opcode_39,
    3.9 : opcode_39,

    '2.6pypy':  opcode_26pypy,
    '2.7pypy':  opcode_27pypy,
    '3.2pypy':  opcode_32pypy,
    '3.3pypy':  opcode_33pypy,
    '3.5pypy':  opcode_35pypy,
    '3.6pypy':  opcode_36pypy,
    '3.6.1pypy':  opcode_36pypy,
    }

for k, v in canonic_python_version.items():
    if v in op_imports:
        op_imports[k] = op_imports[v]

def get_opcode_module(version_info=None, variant=None):
    # FIXME: DRY with magics.sysinfo2float()
    if version_info is None:
        version_info = sys.version_info
        if variant is None and IS_PYPY:
            variant = 'pypy'
            pass
        pass
    elif isinstance(version_info, float):
        int_vers = int(version_info * 10)
        version_info = [int_vers // 10, int_vers % 10]

    vers_str = '.'.join([str(v) for v in version_info[0:3]])
    if len(version_info) >= 3 and version_info[3] != 'final':
        vers_str += ''.join([str(v) for v in version_info[3:]])
    if variant is None:
        try:
            import platform
            variant = platform.python_implementation()
            if platform in ('Jython', 'Pyston'):
                vers_str += variant
                pass
        except ImportError:
            # Python may be too old, e.g. < 2.6 or implementation may
            # just not have platform
            pass
    else:
        vers_str += variant

    return op_imports[canonic_python_version[vers_str]]


if __name__ == '__main__':
    print(get_opcode_module())
