# Copyright (C) 2018-2020 Rocky Bernstein <rocky@gnu.org>
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
from __future__ import print_function
import datetime, py_compile, os, subprocess, sys, tempfile

from uncompyle6 import verify, IS_PYPY, PYTHON_VERSION
from xdis import iscode, sysinfo2float
from uncompyle6.disas import check_object_path
from uncompyle6.semantics import pysource
from uncompyle6.parser import ParserError
from uncompyle6.version import __version__

# from uncompyle6.linenumbers import line_number_mapping

from uncompyle6.semantics.pysource import code_deparse
from uncompyle6.semantics.fragments import code_deparse as code_deparse_fragments
from uncompyle6.semantics.linemap import deparse_code_with_map

from xdis.load import load_module


def _get_outstream(outfile):
    dir = os.path.dirname(outfile)
    failed_file = outfile + "_failed"
    if os.path.exists(failed_file):
        os.remove(failed_file)
    try:
        os.makedirs(dir)
    except OSError:
        pass
    if PYTHON_VERSION < 3.0:
        return open(outfile, mode="wb")
    else:
        return open(outfile, mode="w", encoding="utf-8")


def decompile(
    bytecode_version,
    co,
    out=None,
    showasm=None,
    showast={},
    timestamp=None,
    showgrammar=False,
    source_encoding=None,
    code_objects={},
    source_size=None,
    is_pypy=None,
    magic_int=None,
    mapstream=None,
    do_fragments=False,
):
    """
    ingests and deparses a given code block 'co'

    if `bytecode_version` is None, use the current Python intepreter
    version.

    Caller is responsible for closing `out` and `mapstream`
    """
    if bytecode_version is None:
        bytecode_version = sysinfo2float()

    # store final output stream for case of error
    real_out = out or sys.stdout

    def write(s):
        s += "\n"
        real_out.write(s)

    assert iscode(co)

    co_pypy_str = "PyPy " if is_pypy else ""
    run_pypy_str = "PyPy " if IS_PYPY else ""
    sys_version_lines = sys.version.split("\n")
    if source_encoding:
        write("# -*- coding: %s -*-" % source_encoding)
    write(
        "# uncompyle6 version %s\n"
        "# %sPython bytecode %s%s\n# Decompiled from: %sPython %s"
        % (
            __version__,
            co_pypy_str,
            bytecode_version,
            " (%s)" % str(magic_int) if magic_int else "",
            run_pypy_str,
            "\n# ".join(sys_version_lines),
        )
    )
    if PYTHON_VERSION < 3.0 and bytecode_version >= 3.0:
        write(
            '# Warning: this version of Python has problems handling the Python 3 "byte" type in constants properly.\n'
        )

    if co.co_filename:
        write("# Embedded file name: %s" % co.co_filename,)
    if timestamp:
        write("# Compiled at: %s" % datetime.datetime.fromtimestamp(timestamp))
    if source_size:
        write("# Size of source mod 2**32: %d bytes" % source_size)

    debug_opts = {"asm": showasm, "ast": showast, "grammar": showgrammar}

    try:
        if mapstream:
            if isinstance(mapstream, str):
                mapstream = _get_outstream(mapstream)

            deparsed = deparse_code_with_map(
                co,
                out,
                bytecode_version,
                debug_opts,
                code_objects=code_objects,
                is_pypy=is_pypy,
            )
            header_count = 3 + len(sys_version_lines)
            linemap = [
                (line_no, deparsed.source_linemap[line_no] + header_count)
                for line_no in sorted(deparsed.source_linemap.keys())
            ]
            mapstream.write("\n\n# %s\n" % linemap)
        else:
            if do_fragments:
                deparse_fn = code_deparse_fragments
            else:
                deparse_fn = code_deparse
            deparsed = deparse_fn(
                co, out, bytecode_version, debug_opts=debug_opts, is_pypy=is_pypy
            )
            pass
        return deparsed
    except pysource.SourceWalkerError as e:
        # deparsing failed
        raise pysource.SourceWalkerError(str(e))


def compile_file(source_path):
    if source_path.endswith(".py"):
        basename = source_path[:-3]
    else:
        basename = source_path

    if hasattr(sys, "pypy_version_info"):
        bytecode_path = "%s-pypy%s.pyc" % (basename, PYTHON_VERSION)
    else:
        bytecode_path = "%s-%s.pyc" % (basename, PYTHON_VERSION)

    print("compiling %s to %s" % (source_path, bytecode_path))
    py_compile.compile(source_path, bytecode_path, "exec")
    return bytecode_path


def decompile_file(
    filename,
    outstream=None,
    showasm=None,
    showast=False,
    showgrammar=False,
    source_encoding=None,
    mapstream=None,
    do_fragments=False,
):
    """
    decompile Python byte-code file (.pyc). Return objects to
    all of the deparsed objects found in `filename`.
    """

    filename = check_object_path(filename)
    code_objects = {}
    (version, timestamp, magic_int, co, is_pypy, source_size, sip_hash) = load_module(
        filename, code_objects
    )

    if isinstance(co, list):
        deparsed = []
        for con in co:
            deparsed.append(
                decompile(
                    version,
                    con,
                    outstream,
                    showasm,
                    showast,
                    timestamp,
                    showgrammar,
                    source_encoding,
                    code_objects=code_objects,
                    is_pypy=is_pypy,
                    magic_int=magic_int,
                ),
                mapstream=mapstream,
            )
    else:
        deparsed = [
            decompile(
                version,
                co,
                outstream,
                showasm,
                showast,
                timestamp,
                showgrammar,
                source_encoding,
                code_objects=code_objects,
                source_size=source_size,
                is_pypy=is_pypy,
                magic_int=magic_int,
                mapstream=mapstream,
                do_fragments=do_fragments,
            )
        ]
    co = None
    return deparsed


# FIXME: combine into an options parameter
def main(
    in_base,
    out_base,
    compiled_files,
    source_files,
    outfile=None,
    showasm=None,
    showast=False,
    do_verify=False,
    showgrammar=False,
    source_encoding=None,
    raise_on_error=False,
    do_linemaps=False,
    do_fragments=False,
):
    """
    in_base	base directory for input files
    out_base	base directory for output files (ignored when
    files	list of filenames to be uncompyled (relative to in_base)
    outfile	write output to this filename (overwrites out_base)

    For redirecting output to
    - <filename>		outfile=<filename> (out_base is ignored)
    - files below out_base	out_base=...
    - stdout			out_base=None, outfile=None
    """
    tot_files = okay_files = failed_files = verify_failed_files = 0
    current_outfile = outfile
    linemap_stream = None

    for source_path in source_files:
        compiled_files.append(compile_file(source_path))

    for filename in compiled_files:
        infile = os.path.join(in_base, filename)
        # print("XXX", infile)
        if not os.path.exists(infile):
            sys.stderr.write("File '%s' doesn't exist. Skipped\n" % infile)
            continue

        if do_linemaps:
            linemap_stream = infile + ".pymap"
            pass

        # print (infile, file=sys.stderr)

        if outfile:  # outfile was given as parameter
            outstream = _get_outstream(outfile)
        elif out_base is None:
            outstream = sys.stdout
            if do_linemaps:
                linemap_stream = sys.stdout
            if do_verify:
                prefix = os.path.basename(filename) + "-"
                if prefix.endswith(".py"):
                    prefix = prefix[: -len(".py")]

                # Unbuffer output if possible
                buffering = -1 if sys.stdout.isatty() else 0
                if PYTHON_VERSION >= 3.5:
                    t = tempfile.NamedTemporaryFile(
                        mode="w+b", buffering=buffering, suffix=".py", prefix=prefix
                    )
                else:
                    t = tempfile.NamedTemporaryFile(
                        mode="w+b", suffix=".py", prefix=prefix
                    )
                current_outfile = t.name
                sys.stdout = os.fdopen(sys.stdout.fileno(), "w", buffering)
                tee = subprocess.Popen(["tee", current_outfile], stdin=subprocess.PIPE)
                os.dup2(tee.stdin.fileno(), sys.stdout.fileno())
                os.dup2(tee.stdin.fileno(), sys.stderr.fileno())
        else:
            if filename.endswith(".pyc"):
                current_outfile = os.path.join(out_base, filename[0:-1])
            else:
                current_outfile = os.path.join(out_base, filename) + "_dis"
                pass
            pass

            outstream = _get_outstream(current_outfile)

        # print(current_outfile, file=sys.stderr)

        # Try to uncompile the input file
        try:
            deparsed = decompile_file(
                infile,
                outstream,
                showasm,
                showast,
                showgrammar,
                source_encoding,
                linemap_stream,
                do_fragments,
            )
            if do_fragments:
                for d in deparsed:
                    last_mod = None
                    offsets = d.offsets
                    for e in sorted(
                        [k for k in offsets.keys() if isinstance(k[1], int)]
                    ):
                        if e[0] != last_mod:
                            line = "=" * len(e[0])
                            outstream.write("%s\n%s\n%s\n" % (line, e[0], line))
                        last_mod = e[0]
                        info = offsets[e]
                        extractInfo = d.extract_node_info(info)
                        outstream.write("%s" % info.node.format().strip() + "\n")
                        outstream.write(extractInfo.selectedLine + "\n")
                        outstream.write(extractInfo.markerLine + "\n\n")
                    pass
                pass
            tot_files += 1
        except (ValueError, SyntaxError, ParserError, pysource.SourceWalkerError) as e:
            sys.stdout.write("\n")
            sys.stderr.write("\n# file %s\n# %s\n" % (infile, e))
            failed_files += 1
            tot_files += 1
        except KeyboardInterrupt:
            if outfile:
                outstream.close()
                os.remove(outfile)
            sys.stdout.write("\n")
            sys.stderr.write("\nLast file: %s   " % (infile))
            raise
        except RuntimeError as e:
            sys.stdout.write("\n%s\n" % str(e))
            if str(e).startswith("Unsupported Python"):
                sys.stdout.write("\n")
                sys.stderr.write(
                    "\n# Unsupported bytecode in file %s\n# %s\n" % (infile, e)
                )
            else:
                if outfile:
                    outstream.close()
                    os.remove(outfile)
                sys.stdout.write("\n")
                sys.stderr.write("\nLast file: %s   " % (infile))
                raise

        # except:
        #     failed_files += 1
        #     if current_outfile:
        #         outstream.close()
        #         os.rename(current_outfile, current_outfile + "_failed")
        #     else:
        #         sys.stderr.write("\n# %s" % sys.exc_info()[1])
        #         sys.stderr.write("\n# Can't uncompile %s\n" % infile)
        else:  # uncompile successful
            if current_outfile:
                outstream.close()

                if do_verify:
                    try:
                        msg = verify.compare_code_with_srcfile(
                            infile, current_outfile, do_verify
                        )
                        if not current_outfile:
                            if not msg:
                                print("\n# okay decompiling %s" % infile)
                                okay_files += 1
                            else:
                                verify_failed_files += 1
                                print("\n# %s\n\t%s", infile, msg)
                                pass
                        else:
                            okay_files += 1
                            pass
                    except verify.VerifyCmpError as e:
                        print(e)
                        verify_failed_files += 1
                        os.rename(current_outfile, current_outfile + "_unverified")
                        sys.stderr.write("### Error Verifying %s\n" % filename)
                        sys.stderr.write(str(e) + "\n")
                        if not outfile:
                            if raise_on_error:
                                raise
                            pass
                        pass
                    pass
                else:
                    okay_files += 1
                pass
            elif do_verify:
                sys.stderr.write(
                    "\n### uncompile successful, but no file to compare against\n"
                )
                pass
            else:
                okay_files += 1
                if not current_outfile:
                    mess = "\n# okay decompiling"
                    # mem_usage = __memUsage()
                    print(mess, infile)
        if current_outfile:
            sys.stdout.write(
                "%s -- %s\r"
                % (
                    infile,
                    status_msg(
                        do_verify,
                        tot_files,
                        okay_files,
                        failed_files,
                        verify_failed_files,
                        do_verify,
                    ),
                )
            )
            try:
                # FIXME: Something is weird with Pypy here
                sys.stdout.flush()
            except:
                pass
    if current_outfile:
        sys.stdout.write("\n")
        try:
            # FIXME: Something is weird with Pypy here
            sys.stdout.flush()
        except:
            pass
        pass
    return (tot_files, okay_files, failed_files, verify_failed_files)


# ---- main ----

if sys.platform.startswith("linux") and os.uname()[2][:2] in ["2.", "3.", "4."]:

    def __memUsage():
        mi = open("/proc/self/stat", "r")
        mu = mi.readline().split()[22]
        mi.close()
        return int(mu) / 1000000


else:

    def __memUsage():
        return ""


def status_msg(
    do_verify, tot_files, okay_files, failed_files, verify_failed_files, weak_verify
):
    if weak_verify == "weak":
        verification_type = "weak "
    elif weak_verify == "verify-run":
        verification_type = "run "
    else:
        verification_type = ""
    if tot_files == 1:
        if failed_files:
            return "\n# decompile failed"
        elif verify_failed_files:
            return "\n# decompile %sverification failed" % verification_type
        else:
            return "\n# Successfully decompiled file"
            pass
        pass
    mess = "decompiled %i files: %i okay, %i failed" % (
        tot_files,
        okay_files,
        failed_files,
    )
    if do_verify:
        mess += ", %i %sverification failed" % (verify_failed_files, verification_type)
    return mess
