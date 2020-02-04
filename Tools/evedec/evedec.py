'''
evedec.py
Reads and decrypts Eve Online python files and passes them to uncompyle2 to decompile.
  -Doesn't manipulate Eve process. Can be run with or without Eve running.
  -Searches for decryption key in the blue.dll file.
  -Requires uncompyle2 for actual decompilation.
  -Uses multiple processes to speed up decompilation.

Expects a evedec.ini file to specify Eve install location and output directory, e.g.:
[main]
eve_path = C:\Program Files (x86)\CCP\EVE\
store_path = ..\

'''

#function executed by each decompile process
def process_func(code_q, result_q, store_path, lock):
    okay_files = failed_files = 0
    try:
        import sys, os, marshal, errno, Queue
        import uncompyle2
        while 1:
            filename, marshalled_code = code_q.get(True, 5) #give up after 5 sec
            if filename == None: #None is our end marker
                break
            try:
                code = marshal.loads(marshalled_code)
                
                #prepend our store_path
                filename = os.path.join(store_path, filename)
                filename = os.path.abspath(filename)
                try:
                    os.makedirs(os.path.dirname(filename))
                except OSError as e:
                    #the dir may already exist, in which case ignore error
                    if e.errno != errno.EEXIST:
                        raise
                try:
                    os.remove(filename+'_failed')
                except OSError as e:
                    if e.errno != errno.ENOENT:
                        raise
                with open(filename, 'w') as out_file:
                    uncompyle2.uncompyle(2.5, code, out_file)
            except KeyboardInterrupt:
                raise
            except:
                with lock:
                    print '### Can\'t decompile %s' % filename
                    sys.stdout.flush()
                os.rename(filename, filename+'_failed')
                failed_files += 1
            else:
                with lock:
                    print '+++ Okay decompiling %s' % filename
                    sys.stdout.flush()
                okay_files += 1
                
    except Queue.Empty: #timeout reached
        pass
    finally:
        result_q.put((okay_files, failed_files))

#executed once by the starting process
if __name__ == '__main__':
    #moved imports here so that the other processes don't import them unnecessarily
    import sys
    if sys.version[:3] != '2.7':
        print >>sys.stderr, '!!! Wrong Python version : %s.  Python 2.7 required.'
        sys.exit(-1)
    import os, cPickle, imp, zipfile, zlib, traceback
    from Queue import Empty
    from multiprocessing import Process, Queue, cpu_count, freeze_support, Lock
    from datetime import datetime
    from ConfigParser import ConfigParser
    from ctypes import windll, c_void_p, c_int, create_string_buffer, byref

    freeze_support()
    
    startTime = datetime.now() #time this cpu hog
    
    #Get path to Eve installation from evedec.ini file
    config = ConfigParser()
    config.read('evedec.ini')
    eve_path = config.get('main', 'eve_path')
    
    #use version info from eve's common.ini to create directory name
    eveconfig = ConfigParser()
    eveconfig.read(os.path.join(eve_path, 'common.ini'))

    store_path = os.path.join(config.get('main', 'store_path'), \
      'eve-%s.%s' % (eveconfig.get('main', 'version'), eveconfig.get('main', 'build')))
    store_path = os.path.abspath(store_path)

    #search blue.dll for keyblob header
    #yeah, it's really that easy

    blue_path = os.path.join(eve_path, 'bin/blue.dll')
    blue = open(blue_path, 'rb').read()
    blob_header = '010200000366000000A40000'.decode('hex') #simpleblob 3des
    #look for multiple keys, just in case
    #currently there is only one matching byte sequence, so this is overkill
    keylocs = []
    i=0
    while 1:
        i = blue.find(blob_header, i)
        if i == -1 or i+36 >= len(blue):
            break
        i += len(blob_header)
        #parity check, again not really necessary but what the hell
        p = 1 #3des key bytes should have odd parity just like des
        for byte in [ord(c) for c in blue[i:i+24]]:
            byte ^= byte >> 4
            byte ^= byte >> 2
            p &= byte ^ (byte >> 1)

        if p:
            keylocs.append(i)
            
    if keylocs:
        print 'Number of possible keys found: %s' % len(keylocs)
    else:
        print >> sys.stderr, '!!! No keys found in blue.dll.'
        sys.exit(-1)

    #set up crypt api
    #need a context before we can import key
    hProv = c_void_p()
    windll.advapi32.CryptAcquireContextA(byref(hProv), 0, 'Microsoft Enhanced Cryptographic Provider v1.0', 1, 0xf0000000)

    keys = []
    for keyloc in keylocs:
        #build key blob
        #just convert to plaintextkeyblob as it's a little simpler to import
        # win2000 doesn't support plaintextkeyblob, if you're using win2000 you have bigger problems
        keyblob = '080200000366000018000000'.decode('hex') #plaintextkeyblob; 3des; length(0x18 = 24 bytes = 196 bit)
        keyblob += blue[keyloc:keyloc+24][::-1] #reverse key byte order when converting from simpleblob to plaintextkeyblob
        
        #import the keyblob and get the key handle
        hKey = c_void_p()
        windll.advapi32.CryptImportKey(hProv, keyblob, len(keyblob), 0, 0, byref(hKey))
        keys.append((hKey, blue[keyloc-len(blob_header):keyloc+24], keyblob))

    for key in keys:
        simple, plain = key[1], key[2]
        print
        print '[                     SIMPLEBLOB (as found in blue.dll)                         ]'
        print '[    publickeystruc   ]'
        print '[type ver  res  alg_id] [alg_id] [                encryptedkey                  ]'
        print '   %s  %s %s %s %s %s' % \
              (simple[0].encode('hex'),
              simple[1].encode('hex'),
              simple[2:4][::-1].encode('hex'),
              simple[4:8][::-1].encode('hex'),
              simple[8:12][::-1].encode('hex'),
              simple[12:].encode('hex'))
        print
        print '[         PLAINTEXTKEYBLOB (converted from above simpleblob for import)            ]'
        print '[          hdr        ]'
        print '[type ver  res  alg_id] [dwKeySize] [                  rgbKeyData                  ]'
        print '   %s  %s %s %s    %s %s' % \
              (plain[0].encode('hex'),
              plain[1].encode('hex'),
              plain[2:4][::-1].encode('hex'),
              plain[4:8][::-1].encode('hex'),
              plain[8:12][::-1].encode('hex'),
              plain[12:].encode('hex'))
        print

    CryptDecrypt = windll.advapi32.CryptDecrypt

    def UnjumbleString(s):
        try:
            bData = create_string_buffer(s)
            bDataLen = c_int(len(s))
            CryptDecrypt(keys[0][0], 0, True, 0, bData, byref(bDataLen))
            dec_s = bData.raw[:bDataLen.value] #decrypted string may be shorter, but not longer
            return zlib.decompress(dec_s)
        except zlib.error:
            print 'Key failed. Attempting key switch.'
            del keys[0]
            if not keys:
                print >> sys.stderr, '!!! All keys failed. Exiting.'
                sys.exit(-1)
            return UnjumbleString(s)
            
    
    compiled_path = os.path.join(config.get('main', 'eve_path'), 'script\\compiled.code')
    #queue of marshalled code objects
    code_queue = Queue()
    #queue of process results
    result_queue = Queue()
    
    sys.stdout.flush()
        
    try:
        #create decompile processes
        procs = []
        print_lock = Lock()
        for i in range(cpu_count()-1): #save one process for decompressing/decrypting
            procs.append(Process(target=process_func,
                                 args=(code_queue, result_queue, store_path, print_lock)));
            
        #start procs now; they will block on empty queue
        for p in procs:
            p.start()
            
        # load code from compiled.code and queue it
        f = open(compiled_path, "rb")
        compiled_data = f.read()
        f.close()
        filelist = cPickle.loads(cPickle.loads(compiled_data)[1])['code']

        for code in filelist:
            code_queue.put( ("compiled.code/" + code[0].replace('../', '').replace('script:/', ''), UnjumbleString(code[1][0])) )

        # decompyle zip files too
        with zipfile.ZipFile(os.path.join(eve_path, 'lib\evelib.ccp'), 'r') as zf:
            for filename in zf.namelist():
                if filename[-4:] == '.pyj':
                    code_queue.put( ("evelib.ccp/" + filename[:-1], UnjumbleString(zf.read(filename))[8:]) )
                elif filename[-4:] == '.pyc':
                    code_queue.put( ("evelib.ccp/" + filename[:-1], zf.read(filename)[8:]) )
                    
        with zipfile.ZipFile(os.path.join(eve_path, 'lib\corestdlib.ccp'), 'r') as zf:
            for filename in zf.namelist():
                if filename[-4:] == '.pyj':
                    code_queue.put( ("corestdlib.ccp/" + filename[:-1], UnjumbleString(zf.read(filename))[8:]) )
                elif filename[-4:] == '.pyc':
                    code_queue.put( ("corestdlib.ccp/" + filename[:-1], zf.read(filename)[8:]) )

        with zipfile.ZipFile(os.path.join(eve_path, 'lib\corelib.ccp'), 'r') as zf:
            for filename in zf.namelist():
                if filename[-4:] == '.pyj':
                    code_queue.put( ("corelib.ccp/" + filename[:-1], UnjumbleString(zf.read(filename))[8:]) )
                elif filename[-4:] == '.pyc':
                    code_queue.put( ("corelib.ccp/" + filename[:-1], zf.read(filename)[8:]) )
                    
        #this process is done except for waiting, so add one more decompile process
        p = Process(target=process_func,
                    args=(code_queue, result_queue, store_path, print_lock))
        p.start()
        procs.append(p)
        
        #add sentinel values to indicate end of queue
        for p in procs:
            code_queue.put( (None, None) )

        #wait for decompile processes to finish
        for p in procs:
            p.join() #join() will block until p is finished
        #pull results from the result queue
        okay_files = failed_files = 0
        try:
            while 1: #will terminate when queue.get() generates Empty exception
                (o, f) = result_queue.get(False)
                okay_files += o
                failed_files += f
        except Empty:
            pass
        print '# decompiled %i files: %i okay, %i failed' % \
              (okay_files + failed_files, okay_files, failed_files)
        print '# elapsed time:', datetime.now() - startTime
    except:
        traceback.print_exc()
        os._exit(0) #make Ctrl-C actually end process    
