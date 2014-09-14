@echo off
echo Generating functions from Apache Thrift data...
thrift --gen csharp Line.thrift
echo Done!
pause