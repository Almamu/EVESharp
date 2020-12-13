#  Copyright (c) 2018-2019 by Rocky Bernstein
#
#  This program is free software: you can redistribute it and/or modify
#  it under the terms of the GNU General Public License as published by
#  the Free Software Foundation, either version 3 of the License, or
#  (at your option) any later version.
#
#  This program is distributed in the hope that it will be useful,
#  but WITHOUT ANY WARRANTY; without even the implied warranty of
#  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
#  GNU General Public License for more details.
#
#  You should have received a copy of the GNU General Public License
#  along with this program.  If not, see <http://www.gnu.org/licenses/>.

"""Isolate Python version-specific semantic actions here.
"""

from uncompyle6.semantics.consts import PRECEDENCE, TABLE_R, TABLE_DIRECT

from uncompyle6.parsers.treenode import SyntaxTree
from uncompyle6.scanners.tok import Token


def customize_for_version(self, is_pypy, version):
    if is_pypy:
        ########################
        # PyPy changes
        #######################
        TABLE_DIRECT.update({
            'assert_pypy':	( '%|assert %c\n' ,     (1, 'assert_expr') ),
            # This is as a result of an if transoration
            'assert0_pypy':	( '%|assert %c\n' ,     (0, 'assert_expr') ),

            'assert_not_pypy':	( '%|assert not %c\n' , (1, 'assert_exp') ),
            'assert2_not_pypy':	( '%|assert not %c, %c\n' , (1, 'assert_exp'),
                                  (4, 'expr') ),
            'assert2_pypy':	( '%|assert %c, %c\n' , (1, 'assert_expr'),
                                  (4, 'expr') ),
            'try_except_pypy':	   ( '%|try:\n%+%c%-%c\n\n', 1, 2 ),
            'tryfinallystmt_pypy': ( '%|try:\n%+%c%-%|finally:\n%+%c%-\n\n', 1, 3 ),
            'assign3_pypy':        ( '%|%c, %c, %c = %c, %c, %c\n', 5, 4, 3, 0, 1, 2 ),
            'assign2_pypy':        ( '%|%c, %c = %c, %c\n', 3, 2, 0, 1),
            })
    else:
        ########################
        # Without PyPy
        #######################
        TABLE_DIRECT.update({
            # "assert" and "assert_expr" are added via transform rules.
            "assert": ("%|assert %c\n", 0),
            "assert2": ("%|assert %c, %c\n", 0, 3),

            # Created only via transformation
            "assertnot": ("%|assert not %p\n", (0, PRECEDENCE['unary_not'])),
            "assert2not": ( "%|assert not %p, %c\n" ,
                            (0, PRECEDENCE['unary_not']), 3 ),

            "assign2": ("%|%c, %c = %c, %c\n", 3, 4, 0, 1),
            "assign3": ("%|%c, %c, %c = %c, %c, %c\n", 5, 6, 7, 0, 1, 2),
            "try_except": ("%|try:\n%+%c%-%c\n\n", 1, 3),
        })
    if version >= 3.0:
        if version >= 3.2:
            TABLE_DIRECT.update(
                {"del_deref_stmt": ("%|del %c\n", 0), "DELETE_DEREF": ("%{pattr}", 0)}
            )
        from uncompyle6.semantics.customize3 import customize_for_version3

        customize_for_version3(self, version)
    else:  # < 3.0
        TABLE_DIRECT.update(
            {"except_cond3": ("%|except %c, %c:\n", (1, "expr"), (-2, "store"))}
        )
        if version <= 2.6:
            TABLE_DIRECT["testtrue_then"] = TABLE_DIRECT["testtrue"]

        if 2.4 <= version <= 2.6:
            TABLE_DIRECT.update({"comp_for": (" for %c in %c", 3, 1)})
        else:
            TABLE_DIRECT.update({"comp_for": (" for %c in %c%c", 2, 0, 3)})

        if version >= 2.5:
            from uncompyle6.semantics.customize25 import customize_for_version25

            customize_for_version25(self, version)

            if version >= 2.6:
                from uncompyle6.semantics.customize26_27 import (
                    customize_for_version26_27,
                )

                customize_for_version26_27(self, version)
                pass
        else:  # < 2.5
            global NAME_MODULE
            NAME_MODULE = SyntaxTree(
                "stmt",
                [
                    SyntaxTree(
                        "assign",
                        [
                            SyntaxTree(
                                "expr",
                                [
                                    Token(
                                        "LOAD_GLOBAL",
                                        pattr="__name__",
                                        offset=0,
                                        has_arg=True,
                                    )
                                ],
                            ),
                            SyntaxTree(
                                "store",
                                [
                                    Token(
                                        "STORE_NAME",
                                        pattr="__module__",
                                        offset=3,
                                        has_arg=True,
                                    )
                                ],
                            ),
                        ],
                    )
                ],
            )
            TABLE_DIRECT.update(
                {
                    "importmultiple": ("%|import %c%c\n", 2, 3),
                    "import_cont": (", %c", 2),
                    "tryfinallystmt": (
                        "%|try:\n%+%c%-%|finally:\n%+%c%-",
                        (1, "suite_stmts_opt"),
                        (5, "suite_stmts_opt"),
                    ),
                }
            )
            if version == 2.4:
                def n_iftrue_stmt24(node):
                    self.template_engine(("%c", 0), node)
                    self.default(node)
                    self.prune()

                self.n_iftrue_stmt24 = n_iftrue_stmt24
            else:  # version <= 2.3:
                TABLE_DIRECT.update({"if1_stmt": ("%|if 1\n%+%c%-", 5)})
                if version <= 2.1:
                    TABLE_DIRECT.update(
                        {
                            "importmultiple": ("%c", 2),
                            # FIXME: not quite right. We have indiividual imports
                            # when there is in fact one: "import a, b, ..."
                            "imports_cont": ("%C%,", (1, 100, "\n")),
                        }
                    )
                    pass
                pass
            pass  # < 2.5

        # < 3.0 continues

        TABLE_R.update(
            {
                "STORE_SLICE+0": ("%c[:]", 0),
                "STORE_SLICE+1": ("%c[%p:]", 0, (1, -1)),
                "STORE_SLICE+2": ("%c[:%p]", 0, (1, -1)),
                "STORE_SLICE+3": ("%c[%p:%p]", 0, (1, -1), (2, -1)),
                "DELETE_SLICE+0": ("%|del %c[:]\n", 0),
                "DELETE_SLICE+1": ("%|del %c[%c:]\n", 0, 1),
                "DELETE_SLICE+2": ("%|del %c[:%c]\n", 0, 1),
                "DELETE_SLICE+3": ("%|del %c[%c:%c]\n", 0, 1, 2),
            }
        )
        TABLE_DIRECT.update({"raise_stmt2": ("%|raise %c, %c\n", 0, 1)})

        # exec as a built-in statement is only in Python 2.x
        def n_exec_stmt(node):
            """
            exec_stmt ::= expr exprlist DUP_TOP EXEC_STMT
            exec_stmt ::= expr exprlist EXEC_STMT
            """
            self.write(self.indent, "exec ")
            self.preorder(node[0])
            if not node[1][0].isNone():
                sep = " in "
                for subnode in node[1]:
                    self.write(sep)
                    sep = ", "
                    self.preorder(subnode)
            self.println()
            self.prune()  # stop recursing

        self.n_exec_smt = n_exec_stmt

        pass  # < 3.0

    return
