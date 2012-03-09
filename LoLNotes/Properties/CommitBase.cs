/*
copyright (C) 2011-2012 by high828@gmail.com

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/
using System;
using System.Linq;
using System.Reflection;

[AttributeUsage(AttributeTargets.Assembly, Inherited = false, AllowMultiple = true)]
public class AssemblyCommitAttribute : Attribute
{
	public AssemblyCommitAttribute(string raw, string hash, string author, string date, string message, string email)
	{
		Raw = raw;
		Hash = hash;
		Author = author;
		Date = date;
		Message = message;
		Email = email;
	}

	public string Raw { get; set; }
	public string Hash { get; set; }
	public string Author { get; set; }
	public string Email { get; set; }
	public string Date { get; set; }
	public string Message { get; set; }
}

public class GitInfo
{
	public static AssemblyCommitAttribute Get()
	{
		return Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCommitAttribute), false).FirstOrDefault() as AssemblyCommitAttribute;
	}
}