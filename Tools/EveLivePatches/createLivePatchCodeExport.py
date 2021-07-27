import marshal
fp = open('C:/development/tmp.marshal', 'wb')

marshal.dump(FunctionName.func_code, fp)
fp.close()
