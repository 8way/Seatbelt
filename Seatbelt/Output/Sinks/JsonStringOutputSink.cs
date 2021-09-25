﻿using System;
using System.IO;
using System.Web.Script.Serialization;
using Seatbelt.Commands;

namespace Seatbelt.Output.Sinks
{
    // Any sinks that output text to a location should inherit from this class
    internal class JsonStringOutputSink : IOutputSink
    {
        private StreamWriter _streamWriter;
        private MemoryStream _stream;

        public JsonStringOutputSink(string file, bool filterResults)
        {
            _stream = new MemoryStream();
            _streamWriter = new StreamWriter(_stream);
            _streamWriter.AutoFlush = true;
        }

        public void WriteOutput(CommandDTOBase dto)
        { 
            //var obj = dtoCollection?.FirstOrDefault();
            if (dto == null)
            {
                return;
            }

            // If the dto has a custom output sink, use it.  Otherwise, use the default output sink
            var dtoType = dto.GetType();
            var json = new JavaScriptSerializer();

            var obj = new
            {
                Type = dtoType.ToString(),
                CommandVersion = dto.GetCommandVersion(),
                Data = dto
            };

            string jsonStr;
            try
            {
                jsonStr = json.Serialize(obj);
            }
            catch(Exception e)
            {
                jsonStr = json.Serialize(new
                {
                    Type = typeof(ErrorDTO).ToString(),
                    Data = json.Serialize(new ErrorDTO(e.ToString()))
                });
            }

            _streamWriter.WriteLine(jsonStr);
        }

        public void WriteVerbose(string message) => WriteOutput(new VerboseDTO(message));

        public void WriteWarning(string message) => WriteOutput(new WarningDTO(message));

        public void WriteError(string message) => WriteOutput(new ErrorDTO(message));

        public void WriteHost(string message) => WriteOutput(new HostDTO(message));

        public string GetOutput()
        {
            _stream.Flush();
            _streamWriter.Flush();
            return System.Text.Encoding.ASCII.GetString(_stream.GetBuffer());
        }

        public void Dispose()
        {
            _streamWriter.Dispose();
            _stream.Dispose();
        }
    }
}