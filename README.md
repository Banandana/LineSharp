# Too much has changed in LINE's protocol to where I WILL NOT update this library. It DOES NOT WORK anymore, and it WILL NOT BE FIXED.

# EDIT: It seems the apache thrift structures embedded in the java code are programmed/encoded differently. Not sure if regenerating an RPC document is possible at this point. (12-04-16)

This library is still hosted on github for historical and documentation purposes and is no longer useful for new development.

This is in no way sponsored, endorsed or administered by, or associated with LINE Corp.
None of the code on this repository has been copied, modified, or derived from LINE Corp's source code, nor does this project intend or condone such acts. The responsibility of the usage of this repository lies with the users and the author cannot be held liable for his or her users' actions.

This is a C# implementation of the LINE protocol. It's currently a WORK IN PROGRESS and I've written it entirely on my own. It's been my personal project for over a year, and it wouldn't exist if it wasn't for the following people:

 - Lumpio
 - Bandocheman

The actual repository containing some info about the protocol can be found elsewhere.

This implementation is fairly simple. I have another repository that contains an actual example of implementation.

TODO:
 - Finish callbacks on operation functions
 - Figure out how 'secure chat' works and implement that as well
 - Random misc items


Unfortunately, this project was not created with a specific purpose in mind. Because of that, it can be used for many different things, including bots, creation of a custom client, etc.

If there are features you wish to see in this implementation, submit a pull request or ask. It could be simple, or it could be incredibly complex. It's typically the former.

- Banandana
