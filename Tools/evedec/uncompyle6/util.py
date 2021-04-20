# Python repr() for complex numbers, and a little broken for floats.
# Until such time it is fixed, we'll do a better.
# More could be done here though.

try:
    from math import copysign
    def is_negative_zero(n):
        """Returns true if n is -0.0"""
        return n == 0.0 and copysign(1, n) == -1
except:
    def is_negative_zero(n):
        return False

from uncompyle6 import PYTHON_VERSION

def better_repr(v, version):
    """Work around Python's unorthogonal and unhelpful repr() for primitive float
    and complex."""
    if isinstance(v, float):
        # float values 'nan' and 'inf' are not directly
        # representable in Python before Python 3.5. In Python 3.5
        # it is accessible via a library constant math.inf.  We
        # will canonicalize representation of these value as
        # float('nan') and float('inf')
        if str(v) in frozenset(["nan", "-nan", "inf", "-inf"]):
            return "float('%s')" % v
        elif is_negative_zero(v):
            return "-0.0"
        return repr(v)
    elif isinstance(v, complex):
        real = better_repr(v.real, version)
        imag = better_repr(v.imag, version)
        # FIXME: we could probably use repr() in most cases
        # sort out when that's possible.
        # The below is however round-tripable, and Python's repr() isn't.
        return "complex(%s, %s)" % (real, imag)
    elif isinstance(v, tuple):
        if len(v) == 1:
            return "(%s,)" % better_repr(v[0], version)
        return "(%s)" % ", ".join(better_repr(i, version) for i in v)
    elif PYTHON_VERSION < 3.0 and isinstance(v, long):
        s = repr(v)
        if version >= 3.0 and s[-1] == "L":
            return s[:-1]
        else:
            return s
    elif isinstance(v, list):
        if len(v) == 1:
            return "[%s,]" % better_repr(v[0], version)
        return "[%s]" % ", ".join(better_repr(i) for i in v)
    # TODO: elif deal with sets and dicts
    else:
        return repr(v)
