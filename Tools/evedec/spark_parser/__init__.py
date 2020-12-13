import sys
from spark_parser.version import VERSION

__version__ = 'SPARK-%s Python2 and Python3 compatible' % VERSION
__docformat__ = 'restructuredtext'

PYTHON3 = (sys.version_info >= (3, 0))

from spark_parser.ast import AST as AST
from spark_parser.ast import GenericASTTraversal as GenericASTTraversal
from spark_parser.ast import GenericASTTraversalPruningException as GenericASTTraversalPruningException
from spark_parser.spark import DEFAULT_DEBUG
from spark_parser.spark import GenericParser as GenericParser
from spark_parser.spark import GenericASTBuilder as GenericASTBuilder
from spark_parser.scanner import GenericScanner as GenericScanner
from spark_parser.scanner import GenericToken as GenericToken
