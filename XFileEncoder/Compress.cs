using System.Linq;
using ComponentAce.Compression.Libs.zlib;
using System.IO;

namespace XFileEncode
{
    public class Compress
    {
        //#define CHUNK 16384
        private const int CHUNK = 16384;
        //The def() routine compresses data from an input file to an output file. The output data will be in the zlib format, which is different from the gzip or zip formats. The zlib format has a very small header of only two bytes to identify it as a zlib stream and to provide decoding information, and a four-byte trailer with a fast check value to verify the integrity of the uncompressed data after decoding.
        static public int DoDeflate(byte[] source, byte[] dest, int level)
        {
        /* Compress from file source to file dest until EOF on source.
           def() returns Z_OK on success, Z_MEM_ERROR if memory could not be
           allocated for processing, Z_STREAM_ERROR if an invalid compression
           level is supplied, Z_VERSION_ERROR if the version of zlib.h and the
           version of the library linked do not match, or Z_ERRNO if there is
           an error reading or writing the files. */

        //Here are the local variables for def(). ret will be used for zlib return codes. 
        //flush will keep track of the current flushing state for deflate(), which is either no flushing, or flush to completion after the end of the input file is reached. 
        //have is the amount of data returned from deflate(). 
        //The strm structure is used to pass information to and from the zlib routines, and to maintain the deflate() state. 
        //in and out are the input and output buffers for deflate().

            if (source == null || dest == null)
            {
                return zlibConst.Z_ERRNO;
            }
            MemoryStream msIn = new MemoryStream(source);

            int ret;
            int flush;
            uint have;
            ZStream strm = new ZStream();
            
        //The first thing we do is to initialize the zlib state for compression using deflateInit(). 
        //This must be done before the first use of deflate(). 
        //The zalloc, zfree, and opaque fields in the strm structure must be initialized before calling deflateInit(). 
        //Here they are set to the zlib constant Z_NULL to request that zlib use the default memory allocation routines. 
        //An application may also choose to provide custom memory allocation routines here. deflateInit() will allocate on the order of 256K bytes for the internal state. (See zlib Technical Details.)
        //deflateInit() is called with a pointer to the structure to be initialized and the compression level, which is an integer in the range of -1 to 9. 
        //Lower compression levels result in faster execution, but less compression. Higher levels result in greater compression, but slower execution. 
        //The zlib constant Z_DEFAULT_COMPRESSION, equal to -1, provides a good compromise between compression and speed and is equivalent to level 6. 
        //Level 0 actually does no compression at all, and in fact expands the data slightly to produce the zlib format (it is not a byte-for-byte copy of the input). 
        //More advanced applications of zlib may use deflateInit2() here instead. Such an application may want to reduce how much memory will be used, at some price in compression. 
        //Or it may need to request a gzip header and trailer instead of a zlib header and trailer, or raw encoding with no header or trailer at all.
        //We must check the return value of deflateInit() against the zlib constant Z_OK to make sure that it was able to allocate memory for the internal state, 
        //and that the provided arguments were valid. deflateInit() will also check that the version of zlib that the zlib.h file came from matches the version of zlib actually linked with the program. 
        //This is especially important for environments in which zlib is a shared library.
        //Note that an application can initialize multiple, independent zlib streams, which can operate in parallel. 
        //The state information maintained in the structure allows the zlib routines to be reentrant.

            /* allocate deflate state */

            ret = strm.deflateInit(level);
            if (ret != zlibConst.Z_OK)
                return ret;
        //With the pleasantries out of the way, now we can get down to business. 
        //The outer do-loop reads all of the input file and exits at the bottom of the loop once end-of-file is reached. 
        //This loop contains the only call of deflate(). So we must make sure that all of the input data has been processed 
        //and that all of the output data has been generated and consumed before we fall out of the loop at the bottom.
               int inCount = 1;
               int inRemain = source.Length;
            /* compress until end of file */
            do {
        //We start off by reading data from the input file. The number of bytes read is put directly into avail_in, and a pointer to those bytes is put into next_in. 
        //We also check to see if end-of-file on the input has been reached using feof(). 
        //If we are at the end of file, then flush is set to the zlib constant Z_FINISH, which is later passed to deflate() to indicate that this is the last chunk of input data to compress. 
        //If we are not yet at the end of the input, then the zlib constant Z_NO_FLUSH will be passed to deflate to indicate that we are still in the middle of the uncompressed data.
        //If there is an error in reading from the input file, the process is aborted with deflateEnd() being called to free the allocated zlib state before returning the error. 
        //We wouldn't want a memory leak, now would we? deflateEnd() can be called at any time after the state has been initialized. 
        //Once that's done, deflateInit() (or deflateInit2()) would have to be called to start a new compression process. There is no point here in checking the deflateEnd() return code. 
        //The deallocation can't fail.
                byte[] inChunk = new byte[CHUNK];
                
                byte[] outChunk = new byte[CHUNK];

                strm.avail_in = CHUNK;
                if (inCount * CHUNK > inRemain)
                {
                    strm.avail_in = inRemain;
                }
                inRemain = source.Length - inCount * CHUNK; //剩余字节数
                inCount++;
                
                for (int i = 0; i < strm.avail_in; i++)
                {
                    inChunk[i] = source[i];
                }
                source.Skip(strm.avail_in);
                //strm.avail_in = inCount * CHUNK > inChunk.Length fread(inChunk, 1, CHUNK, source);
                //if (ferror(source)) {
                //    (void)deflateEnd(&strm);
                //    return Z_ERRNO;
                //}
                //flush = feof(source) ? Z_FINISH : Z_NO_FLUSH;
                flush = strm.avail_in == 0 ? zlibConst.Z_FINISH : zlibConst.Z_NO_FLUSH;
                strm.next_in = inChunk;
        //The inner do-loop passes our chunk of input data to deflate(), and then keeps calling deflate() until it is done producing output. 
        //Once there is no more new output, deflate() is guaranteed to have consumed all of the input, i.e., avail_in will be zero.

                /* run deflate() on input until output buffer not full, finish
                   compression if all of source has been read in */
                do {
        //Output space is provided to deflate() by setting avail_out to the number of available output bytes and next_out to a pointer to that space.

                    strm.avail_out = CHUNK;
                    strm.next_out = outChunk;
        //Now we call the compression engine itself, deflate(). It takes as many of the avail_in bytes at next_in as it can process, and writes as many as avail_out bytes to next_out. 
        //Those counters and pointers are then updated past the input data consumed and the output data written. 
        //It is the amount of output space available that may limit how much input is consumed. 
        //Hence the inner loop to make sure that all of the input is consumed by providing more output space each time. 
        //Since avail_in and next_in are updated by deflate(), we don't have to mess with those between deflate() calls until it's all used up.
        //The parameters to deflate() are a pointer to the strm structure containing the input and output information and the internal compression engine state, 
        //and a parameter indicating whether and how to flush data to the output. Normally deflate will consume several K bytes of input data before producing any output (except for the header), 
        //in order to accumulate statistics on the data for optimum compression. It will then put out a burst of compressed data, and proceed to consume more input before the next burst. 
        //Eventually, deflate() must be told to terminate the stream, complete the compression with provided input data, and write out the trailer check value. 
        //deflate() will continue to compress normally as long as the flush parameter is Z_NO_FLUSH. Once the Z_FINISH parameter is provided, 
        //deflate() will begin to complete the compressed output stream. However depending on how much output space is provided, 
        //deflate() may have to be called several times until it has provided the complete compressed stream, even after it has consumed all of the input. 
        //The flush parameter must continue to be Z_FINISH for those subsequent calls.
        //There are other values of the flush parameter that are used in more advanced applications. 
        //You can force deflate() to produce a burst of output that encodes all of the input data provided so far, 
        //even if it wouldn't have otherwise, for example to control data latency on a link with compressed data. 
        //You can also ask that deflate() do that as well as erase any history up to that point so that what follows can be decompressed independently, 
        //for example for random access applications. Both requests will degrade compression by an amount depending on how often such requests are made.
        //deflate() has a return value that can indicate errors, yet we do not check it here. Why not? Well, it turns out that deflate() can do no wrong here. 
        //Let's go through deflate()'s return values and dispense with them one by one. The possible values are Z_OK, Z_STREAM_END, Z_STREAM_ERROR, or Z_BUF_ERROR. 
        //Z_OK is, well, ok. Z_STREAM_END is also ok and will be returned for the last call of deflate(). 
        //This is already guaranteed by calling deflate() with Z_FINISH until it has no more output. 
        //Z_STREAM_ERROR is only possible if the stream is not initialized properly, but we did initialize it properly. 
        //There is no harm in checking for Z_STREAM_ERROR here,
        //for example to check for the possibility that some other part of the application inadvertently clobbered the memory containing the zlib state. 
        //Z_BUF_ERROR will be explained further below, but suffice it to say that this is simply an indication that deflate() could not consume more input or produce more output. 
        //deflate() can be called again with more output space or more available input, which it will be in this code.

                    ret = strm.deflate(flush);    /* no bad return value */

        //Now we compute how much output deflate() provided on the last call, which is the difference between how much space was provided before the call, 
        //and how much output space is still available after the call. Then that data, if any, is written to the output file. We can then reuse the output buffer for the next call of deflate(). 
        //Again if there is a file i/o error, we call deflateEnd() before returning to avoid a memory leak.

                    have = (uint)(CHUNK - strm.avail_out);
                    dest = (byte[])dest.Concat(outChunk);
                    //if (fwrite(out, 1, have, dest) != have || ferror(dest)) {
                    //    (void)deflateEnd(&strm);
                    //    return Z_ERRNO;
                    //}
        //The inner do-loop is repeated until the last deflate() call fails to fill the provided output buffer. 
        //Then we know that deflate() has done as much as it can with the provided input, and that all of that input has been consumed. 
        //We can then fall out of this loop and reuse the input buffer.
        //The way we tell that deflate() has no more output is by seeing that it did not fill the output buffer, leaving avail_out greater than zero. 
        //However suppose that deflate() has no more output, but just so happened to exactly fill the output buffer! 
        //avail_out is zero, and we can't tell that deflate() has done all it can. As far as we know, deflate() has more output for us. So we call it again. 
        //But now deflate() produces no output at all, and avail_out remains unchanged as CHUNK. 
        //That deflate() call wasn't able to do anything, either consume input or produce output, and so it returns Z_BUF_ERROR. (See, I told you I'd cover this later.) 
        //However this is not a problem at all. Now we finally have the desired indication that deflate() is really done, and so we drop out of the inner loop to provide more input to deflate().
        //With flush set to Z_FINISH, this final set of deflate() calls will complete the output stream. 
        //Once that is done, subsequent calls of deflate() would return Z_STREAM_ERROR if the flush parameter is not Z_FINISH, and do no more processing until the state is reinitialized.
        //Some applications of zlib have two loops that call deflate() instead of the single inner loop we have here. 
        //The first loop would call without flushing and feed all of the data to deflate(). The second loop would call deflate() with no more data and the Z_FINISH parameter to complete the process. 
        //As you can see from this example, that can be avoided by simply keeping track of the current flush state.

                } while (strm.avail_out == 0);
                //assert(strm.avail_in == 0);     /* all input will be used */
        //Now we check to see if we have already processed all of the input file. That information was saved in the flush variable, so we see if that was set to Z_FINISH. 
        //If so, then we're done and we fall out of the outer loop. 
        //We're guaranteed to get Z_STREAM_END from the last deflate() call, since we ran it until the last chunk of input was consumed and all of the output was generated.

                /* done when last data in file processed */
            } while (flush != zlibConst.Z_FINISH);
            //assert(ret == Z_STREAM_END);        /* stream will be complete */
        //The process is complete, but we still need to deallocate the state to avoid a memory leak (or rather more like a memory hemorrhage if you didn't do this). 
        //Then finally we can return with a happy return value.

            /* clean up and return */
            strm.deflateEnd();
            return zlibConst.Z_OK;
        }
    }


}
