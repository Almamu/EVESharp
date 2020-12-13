#  Copyright (c) 2016-2020 Rocky Bernstein
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
"""
spark grammar differences over Python 3.5 for Python 3.6.
"""
from __future__ import print_function

from uncompyle6.parser import PythonParserSingle, nop_func
from spark_parser import DEFAULT_DEBUG as PARSER_DEFAULT_DEBUG
from uncompyle6.parsers.parse35 import Python35Parser
from uncompyle6.scanners.tok import Token

class Python36Parser(Python35Parser):

    def __init__(self, debug_parser=PARSER_DEFAULT_DEBUG):
        super(Python36Parser, self).__init__(debug_parser)
        self.customized = {}


    def p_36misc(self, args):
        """sstmt ::= sstmt RETURN_LAST

        # long except clauses in a loop can sometimes cause a JUMP_BACK to turn into a
        # JUMP_FORWARD to a JUMP_BACK. And when this happens there is an additional
        # ELSE added to the except_suite. With better flow control perhaps we can
        # sort this out better.
        except_suite ::= c_stmts_opt POP_EXCEPT jump_except ELSE
        except_suite_finalize ::= SETUP_FINALLY c_stmts_opt except_var_finalize END_FINALLY
                                  _jump ELSE

        # 3.6 redoes how return_closure works. FIXME: Isolate to LOAD_CLOSURE
        return_closure   ::= LOAD_CLOSURE DUP_TOP STORE_NAME RETURN_VALUE RETURN_LAST

        for_block       ::= l_stmts_opt come_from_loops JUMP_BACK
        come_from_loops ::= COME_FROM_LOOP*

        whilestmt       ::= SETUP_LOOP testexpr l_stmts_opt
                            JUMP_BACK come_froms POP_BLOCK COME_FROM_LOOP
        whilestmt       ::= SETUP_LOOP testexpr l_stmts_opt
                            come_froms JUMP_BACK come_froms POP_BLOCK COME_FROM_LOOP

        # 3.6 due to jump optimization, we sometimes add RETURN_END_IF where
        # RETURN_VALUE is meant. Specifcally this can happen in
        # ifelsestmt -> ...else_suite _. suite_stmts... (last) stmt
        return ::= ret_expr RETURN_END_IF
        return ::= ret_expr RETURN_VALUE COME_FROM
        return_stmt_lambda ::= ret_expr RETURN_VALUE_LAMBDA COME_FROM

        # A COME_FROM is dropped off because of JUMP-to-JUMP optimization
        and  ::= expr jmp_false expr
        and  ::= expr jmp_false expr jmp_false

        jf_cf       ::= JUMP_FORWARD COME_FROM
        cf_jf_else  ::= come_froms JUMP_FORWARD ELSE

        if_exp ::= expr jmp_false expr jf_cf expr COME_FROM

        async_for_stmt     ::= SETUP_LOOP expr
                               GET_AITER
                               LOAD_CONST YIELD_FROM SETUP_EXCEPT GET_ANEXT LOAD_CONST
                               YIELD_FROM
                               store
                               POP_BLOCK JUMP_FORWARD COME_FROM_EXCEPT DUP_TOP
                               LOAD_GLOBAL COMPARE_OP POP_JUMP_IF_FALSE
                               POP_TOP POP_TOP POP_TOP POP_EXCEPT POP_BLOCK
                               JUMP_ABSOLUTE END_FINALLY COME_FROM
                               for_block POP_BLOCK
                               COME_FROM_LOOP

        stmt      ::= async_for_stmt36

        async_for_stmt36   ::= SETUP_LOOP expr
                               GET_AITER
                               LOAD_CONST YIELD_FROM
                               SETUP_EXCEPT GET_ANEXT LOAD_CONST
                               YIELD_FROM
                               store
                               POP_BLOCK JUMP_BACK COME_FROM_EXCEPT DUP_TOP
                               LOAD_GLOBAL COMPARE_OP POP_JUMP_IF_TRUE
                               END_FINALLY for_block
                               COME_FROM
                               POP_TOP POP_TOP POP_TOP POP_EXCEPT
                               POP_TOP POP_BLOCK
                               COME_FROM_LOOP

        async_forelse_stmt ::= SETUP_LOOP expr
                               GET_AITER
                               LOAD_CONST YIELD_FROM SETUP_EXCEPT GET_ANEXT LOAD_CONST
                               YIELD_FROM
                               store
                               POP_BLOCK JUMP_FORWARD COME_FROM_EXCEPT DUP_TOP
                               LOAD_GLOBAL COMPARE_OP POP_JUMP_IF_FALSE
                               POP_TOP POP_TOP POP_TOP POP_EXCEPT POP_BLOCK
                               JUMP_ABSOLUTE END_FINALLY COME_FROM
                               for_block POP_BLOCK
                               else_suite COME_FROM_LOOP

        # Adds a COME_FROM_ASYNC_WITH over 3.5
        # FIXME: remove corresponding rule for 3.5?

        except_suite ::= c_stmts_opt COME_FROM POP_EXCEPT jump_except COME_FROM

        jb_cfs      ::= JUMP_BACK come_froms

        # If statement inside a loop.
        stmt                ::= ifstmtl
        ifstmtl            ::= testexpr _ifstmts_jumpl
        _ifstmts_jumpl     ::= c_stmts JUMP_BACK

        ifelsestmtl ::= testexpr c_stmts_opt jb_cfs else_suitel
        ifelsestmtl ::= testexpr c_stmts_opt cf_jf_else else_suitel
        ifelsestmt  ::= testexpr c_stmts_opt cf_jf_else else_suite _come_froms
        ifelsestmt  ::= testexpr c_stmts come_froms else_suite come_froms

        # In 3.6+, A sequence of statements ending in a RETURN can cause
        # JUMP_FORWARD END_FINALLY to be omitted from try middle

        except_return    ::= POP_TOP POP_TOP POP_TOP returns
        except_handler   ::= JUMP_FORWARD COME_FROM_EXCEPT except_return

        # Try middle following a returns
        except_handler36 ::= COME_FROM_EXCEPT except_stmts END_FINALLY

        stmt             ::= try_except36
        try_except36     ::= SETUP_EXCEPT returns except_handler36
                             opt_come_from_except
        try_except36     ::= SETUP_EXCEPT suite_stmts
        try_except36     ::= SETUP_EXCEPT suite_stmts_opt POP_BLOCK
                             except_handler36 opt_come_from_except

        # 3.6 omits END_FINALLY sometimes
        except_handler36 ::= COME_FROM_EXCEPT except_stmts
        except_handler36 ::= JUMP_FORWARD COME_FROM_EXCEPT except_stmts
        except_handler   ::= jmp_abs COME_FROM_EXCEPT except_stmts

        stmt             ::= tryfinally36
        tryfinally36     ::= SETUP_FINALLY returns
                             COME_FROM_FINALLY suite_stmts
        tryfinally36     ::= SETUP_FINALLY returns
                             COME_FROM_FINALLY suite_stmts_opt END_FINALLY
        except_suite_finalize ::= SETUP_FINALLY returns
                                  COME_FROM_FINALLY suite_stmts_opt END_FINALLY _jump

        stmt ::= tryfinally_return_stmt
        tryfinally_return_stmt ::= SETUP_FINALLY suite_stmts_opt POP_BLOCK LOAD_CONST
                                   COME_FROM_FINALLY

        compare_chained2 ::= expr COMPARE_OP come_froms JUMP_FORWARD

        """

    # Some of this is duplicated from parse37. Eventually we'll probably rebase from
    # that and then we can remove this.
    def p_37conditionals(self, args):
        """
        expr                       ::= if_exp37
        if_exp37                   ::= expr expr jf_cfs expr COME_FROM
        jf_cfs                     ::= JUMP_FORWARD _come_froms
        ifelsestmt                 ::= testexpr c_stmts_opt jf_cfs else_suite opt_come_from_except
        """

    def customize_grammar_rules(self, tokens, customize):
        # self.remove_rules("""
        # """)
        super(Python36Parser, self).customize_grammar_rules(tokens, customize)
        self.remove_rules("""
           _ifstmts_jumpl     ::= c_stmts_opt
           _ifstmts_jumpl     ::= _ifstmts_jump
           except_handler     ::= JUMP_FORWARD COME_FROM_EXCEPT except_stmts END_FINALLY COME_FROM
           async_for_stmt     ::= SETUP_LOOP expr
                                  GET_AITER
                                  LOAD_CONST YIELD_FROM SETUP_EXCEPT GET_ANEXT LOAD_CONST
                                  YIELD_FROM
                                  store
                                  POP_BLOCK jump_except COME_FROM_EXCEPT DUP_TOP
                                  LOAD_GLOBAL COMPARE_OP POP_JUMP_IF_FALSE
                                  POP_TOP POP_TOP POP_TOP POP_EXCEPT POP_BLOCK
                                  JUMP_ABSOLUTE END_FINALLY COME_FROM
                                  for_block POP_BLOCK JUMP_ABSOLUTE
                                  COME_FROM_LOOP
           async_forelse_stmt ::= SETUP_LOOP expr
                                  GET_AITER
                                  LOAD_CONST YIELD_FROM SETUP_EXCEPT GET_ANEXT LOAD_CONST
                                  YIELD_FROM
                                  store
                                  POP_BLOCK JUMP_FORWARD COME_FROM_EXCEPT DUP_TOP
                                  LOAD_GLOBAL COMPARE_OP POP_JUMP_IF_FALSE
                                  POP_TOP POP_TOP POP_TOP POP_EXCEPT POP_BLOCK
                                  JUMP_ABSOLUTE END_FINALLY COME_FROM
                                  for_block pb_ja
                                  else_suite COME_FROM_LOOP

        """)
        self.check_reduce['call_kw'] = 'AST'

        for i, token in enumerate(tokens):
            opname = token.kind

            if opname == 'FORMAT_VALUE':
                rules_str = """
                    expr              ::= formatted_value1
                    formatted_value1  ::= expr FORMAT_VALUE
                """
                self.add_unique_doc_rules(rules_str, customize)
            elif opname == 'FORMAT_VALUE_ATTR':
                rules_str = """
                expr              ::= formatted_value2
                formatted_value2  ::= expr expr FORMAT_VALUE_ATTR
                """
                self.add_unique_doc_rules(rules_str, customize)
            elif opname == 'MAKE_FUNCTION_8':
                if 'LOAD_DICTCOMP' in self.seen_ops:
                    # Is there something general going on here?
                    rule = """
                       dict_comp ::= load_closure LOAD_DICTCOMP LOAD_STR
                                     MAKE_FUNCTION_8 expr
                                     GET_ITER CALL_FUNCTION_1
                       """
                    self.addRule(rule, nop_func)
                elif 'LOAD_SETCOMP' in self.seen_ops:
                    rule = """
                       set_comp ::= load_closure LOAD_SETCOMP LOAD_STR
                                    MAKE_FUNCTION_8 expr
                                    GET_ITER CALL_FUNCTION_1
                       """
                    self.addRule(rule, nop_func)

            elif opname == 'BEFORE_ASYNC_WITH':
                rules_str = """
                  stmt ::= async_with_stmt
                  async_with_pre     ::= BEFORE_ASYNC_WITH GET_AWAITABLE LOAD_CONST YIELD_FROM SETUP_ASYNC_WITH
                  async_with_post    ::= COME_FROM_ASYNC_WITH
                                         WITH_CLEANUP_START GET_AWAITABLE LOAD_CONST YIELD_FROM
                                         WITH_CLEANUP_FINISH END_FINALLY
                  async_with_as_stmt ::= expr
                               async_with_pre
                               store
                               suite_stmts_opt
                               POP_BLOCK LOAD_CONST
                               async_with_post
                 stmt ::= async_with_as_stmt
                 async_with_stmt ::= expr
                               POP_TOP
                               suite_stmts_opt
                               POP_BLOCK LOAD_CONST
                               async_with_post
                 async_with_stmt ::= expr
                               POP_TOP
                               suite_stmts_opt
                               async_with_post
                """
                self.addRule(rules_str, nop_func)

            elif opname.startswith('BUILD_STRING'):
                v = token.attr
                rules_str = """
                    expr                 ::= joined_str
                    joined_str           ::= %sBUILD_STRING_%d
                """ % ("expr " * v, v)
                self.add_unique_doc_rules(rules_str, customize)
                if 'FORMAT_VALUE_ATTR' in self.seen_ops:
                    rules_str = """
                      formatted_value_attr ::= expr expr FORMAT_VALUE_ATTR expr BUILD_STRING
                      expr                 ::= formatted_value_attr
                    """
                    self.add_unique_doc_rules(rules_str, customize)
            elif opname.startswith('BUILD_MAP_UNPACK_WITH_CALL'):
                v = token.attr
                rule = 'build_map_unpack_with_call ::= %s%s' % ('expr ' * v, opname)
                self.addRule(rule, nop_func)
            elif opname.startswith('BUILD_TUPLE_UNPACK_WITH_CALL'):
                v = token.attr
                rule = ('build_tuple_unpack_with_call ::= ' + 'expr1024 ' * int(v//1024) +
                        'expr32 ' * int((v//32) % 32) +
                        'expr ' * (v % 32) + opname)
                self.addRule(rule, nop_func)
                rule = ('starred ::= %s %s' % ('expr ' * v, opname))
                self.addRule(rule, nop_func)
            elif opname == 'SETUP_ANNOTATIONS':
                # 3.6 Variable Annotations PEP 526
                # This seems to come before STORE_ANNOTATION, and doesn't
                # correspond to direct Python source code.
                rule = """
                    stmt ::= SETUP_ANNOTATIONS
                    stmt ::= ann_assign_init_value
                    stmt ::= ann_assign_no_init

                    ann_assign_init_value ::= expr store store_annotation
                    ann_assign_no_init    ::= store_annotation
                    store_annotation      ::= LOAD_NAME STORE_ANNOTATION
                    store_annotation      ::= subscript STORE_ANNOTATION
                 """
                self.addRule(rule, nop_func)
                # Check to combine assignment + annotation into one statement
                self.check_reduce['assign'] = 'token'
            elif opname == "WITH_CLEANUP_START":
                rules_str = """
                  stmt        ::= with_null
                  with_null   ::= with_suffix
                  with_suffix ::= WITH_CLEANUP_START WITH_CLEANUP_FINISH END_FINALLY
                """
                self.addRule(rules_str, nop_func)
            elif opname == 'SETUP_WITH':
                rules_str = """
                  with       ::= expr SETUP_WITH POP_TOP suite_stmts_opt COME_FROM_WITH
                                 with_suffix

                  # Removes POP_BLOCK LOAD_CONST from 3.6-
                  withasstmt ::= expr SETUP_WITH store suite_stmts_opt COME_FROM_WITH
                                 with_suffix
                  with       ::= expr SETUP_WITH POP_TOP suite_stmts_opt POP_BLOCK
                                 BEGIN_FINALLY COME_FROM_WITH
                                 with_suffix
                """
                self.addRule(rules_str, nop_func)
                pass
            pass
        return

    def custom_classfunc_rule(self, opname, token, customize, next_token, is_pypy):

        args_pos, args_kw = self.get_pos_kw(token)

        # Additional exprs for * and ** args:
        #  0 if neither
        #  1 for CALL_FUNCTION_VAR or CALL_FUNCTION_KW
        #  2 for * and ** args (CALL_FUNCTION_VAR_KW).
        # Yes, this computation based on instruction name is a little bit hoaky.
        nak = ( len(opname)-len('CALL_FUNCTION') ) // 3
        uniq_param = args_kw + args_pos

        if frozenset(('GET_AWAITABLE', 'YIELD_FROM')).issubset(self.seen_ops):
            rule = ('async_call ::= expr ' +
                    ('pos_arg ' * args_pos) +
                    ('kwarg ' * args_kw) +
                    'expr ' * nak + token.kind +
                    ' GET_AWAITABLE LOAD_CONST YIELD_FROM')
            self.add_unique_rule(rule, token.kind, uniq_param, customize)
            self.add_unique_rule('expr ::= async_call', token.kind, uniq_param, customize)

        if opname.startswith('CALL_FUNCTION_KW'):
            if is_pypy:
                # PYPY doesn't follow CPython 3.6 CALL_FUNCTION_KW conventions
                super(Python36Parser, self).custom_classfunc_rule(opname, token, customize, next_token, is_pypy)
            else:
                self.addRule("expr ::= call_kw36", nop_func)
                values = 'expr ' * token.attr
                rule = "call_kw36 ::= expr {values} LOAD_CONST {opname}".format(**locals())
                self.add_unique_rule(rule, token.kind, token.attr, customize)
        elif opname == 'CALL_FUNCTION_EX_KW':
            # Note: this doesn't exist in 3.7 and later
            self.addRule("""expr        ::= call_ex_kw4
                            call_ex_kw4 ::= expr
                                            expr
                                            expr
                                            CALL_FUNCTION_EX_KW
                         """,
                         nop_func)
            if 'BUILD_MAP_UNPACK_WITH_CALL' in self.seen_op_basenames:
                self.addRule("""expr        ::= call_ex_kw
                                call_ex_kw  ::= expr expr build_map_unpack_with_call
                                                CALL_FUNCTION_EX_KW
                             """, nop_func)
            if 'BUILD_TUPLE_UNPACK_WITH_CALL' in self.seen_op_basenames:
                # FIXME: should this be parameterized by EX value?
                self.addRule("""expr        ::= call_ex_kw3
                                call_ex_kw3 ::= expr
                                                build_tuple_unpack_with_call
                                                expr
                                                CALL_FUNCTION_EX_KW
                             """, nop_func)
                if 'BUILD_MAP_UNPACK_WITH_CALL' in self.seen_op_basenames:
                    # FIXME: should this be parameterized by EX value?
                    self.addRule("""expr        ::= call_ex_kw2
                                    call_ex_kw2 ::= expr
                                                    build_tuple_unpack_with_call
                                                    build_map_unpack_with_call
                                                    CALL_FUNCTION_EX_KW
                             """, nop_func)

        elif opname == 'CALL_FUNCTION_EX':
            self.addRule("""
                         expr        ::= call_ex
                         starred     ::= expr
                         call_ex     ::= expr starred CALL_FUNCTION_EX
                         """, nop_func)
            if self.version >= 3.6:
                if 'BUILD_MAP_UNPACK_WITH_CALL' in self.seen_ops:
                    self.addRule("""
                            expr        ::= call_ex_kw
                            call_ex_kw  ::= expr expr
                                            build_map_unpack_with_call CALL_FUNCTION_EX
                            """, nop_func)
                if 'BUILD_TUPLE_UNPACK_WITH_CALL' in self.seen_ops:
                    self.addRule("""
                            expr        ::= call_ex_kw3
                            call_ex_kw3 ::= expr
                                            build_tuple_unpack_with_call
                                            %s
                                            CALL_FUNCTION_EX
                            """ % 'expr ' * token.attr, nop_func)
                    pass

                # FIXME: Is this right?
                self.addRule("""
                            expr        ::= call_ex_kw4
                            call_ex_kw4 ::= expr
                                            expr
                                            expr
                                            CALL_FUNCTION_EX
                            """, nop_func)
            pass
        else:
            super(Python36Parser, self).custom_classfunc_rule(opname, token, customize, next_token, is_pypy)

    def reduce_is_invalid(self, rule, ast, tokens, first, last):
        invalid = super(Python36Parser,
                        self).reduce_is_invalid(rule, ast,
                                                tokens, first, last)
        if invalid:
            return invalid
        if rule[0] == 'assign':
            # Try to combine assignment + annotation into one statement
            if (len(tokens) >= last + 1 and
                tokens[last] == 'LOAD_NAME' and
                tokens[last+1] == 'STORE_ANNOTATION' and
                tokens[last-1].pattr == tokens[last+1].pattr):
                # Will handle as ann_assign_init_value
                return True
            pass
        if rule[0] == 'call_kw':
            # Make sure we don't derive call_kw
            nt = ast[0]
            while not isinstance(nt, Token):
                if nt[0] == 'call_kw':
                    return True
                nt = nt[0]
                pass
            pass
        return False

class Python36ParserSingle(Python36Parser, PythonParserSingle):
    pass

if __name__ == '__main__':
    # Check grammar
    p = Python36Parser()
    p.check_grammar()
    from uncompyle6 import PYTHON_VERSION, IS_PYPY
    if PYTHON_VERSION == 3.6:
        lhs, rhs, tokens, right_recursive, dup_rhs = p.check_sets()
        from uncompyle6.scanner import get_scanner
        s = get_scanner(PYTHON_VERSION, IS_PYPY)
        opcode_set = set(s.opc.opname).union(set(
            """JUMP_BACK CONTINUE RETURN_END_IF COME_FROM
               LOAD_GENEXPR LOAD_ASSERT LOAD_SETCOMP LOAD_DICTCOMP LOAD_CLASSNAME
               LAMBDA_MARKER RETURN_LAST
            """.split()))
        remain_tokens = set(tokens) - opcode_set
        import re
        remain_tokens = set([re.sub(r'_\d+$', '', t) for t in remain_tokens])
        remain_tokens = set([re.sub('_CONT$', '', t) for t in remain_tokens])
        remain_tokens = set(remain_tokens) - opcode_set
        print(remain_tokens)
        # print(sorted(p.rule2name.items()))
