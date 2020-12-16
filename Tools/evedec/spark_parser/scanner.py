"""
Scanning and Token classes that might be useful
in creating specific scanners.
"""

import re

def _namelist(instance):
    namelist, namedict, classlist = [], {}, [instance.__class__]
    for c in classlist:
        for b in c.__bases__:
            classlist.append(b)
        for name in list(c.__dict__.keys()):
            if name not in namedict:
                namelist.append(name)
                namedict[name] = 1
    return namelist

class GenericToken:
    """A sample Token class that can be used in scanning"""
    def __init__(self, kind, attr=None):
        self.kind = kind
        self.attr = attr

    def __eq__(self, o):
        """ '==', but it's okay if offsets and linestarts are different"""
        if isinstance(o, GenericToken):
            return (self.kind == o.kind) and (self.attr == o.attr)
        else:
            return self.kind == o

    def __str__(self):
        if self.attr:
            return 'kind: %s, value: %r' % (self.kind, self.attr)
        else:
            return "kind: %s" % self.kind

    def __repr__(self):
        return self.attr or self.kind

    # Used in generic table-driven semantics routines
    def __hash__(self):
        return hash(self.attr)

    # Used in generic table-driven semantics routines
    def __getitem__(self, i):
        raise IndexError

class GenericScanner:
    """A class which can be used subclass off of to make

    specific sets of scanners. Scanner methods that are subclassed off
    of this that begin with t_ will be introspected in their
    documentation string and uses as a regular expression in a token pattern.
    For example:

        def t_add_op(self, s):
        r'[+-]'
        t = GenericToken(kind='ADD_OP', attr=s)
        self.rv.append(t)
    """
    def __init__(self):
        pattern = self.reflect()
        self.pos = 0
        self.re = re.compile(pattern, re.VERBOSE)

        self.index2func = {}
        for name, number in self.re.groupindex.items():
            self.index2func[number-1] = getattr(self, 't_' + name)

    def makeRE(self, name):
        doc = getattr(self, name).__doc__
        rv = '(?P<%s>%s)' % (name[2:], doc)
        return rv

    def reflect(self):
        rv = []
        for name in list(_namelist(self)):
            if name[:2] == 't_' and name != 't_default':
                rv.append(self.makeRE(name))
        rv.append(self.makeRE('t_default'))
        return '|'.join(rv)

    def error(self, s):
        """Simple-minded error handler. see py2_scan for another
        possibility.'
        """
        print("Lexical error in %s at position %s" % (s, self.pos))
        raise SystemExit

    def tokenize(self, s):
        self.pos = 0
        n = len(s)
        while self.pos < n:
            m = self.re.match(s, self.pos)
            if m is None:
                self.error(s)

            groups = m.groups()
            for i in range(len(groups)):
                if groups[i] and i in self.index2func:
                    self.index2func[i](groups[i])
            self.pos = m.end()

    def t_default(self, s):
        r'( \n )+'
        pass
