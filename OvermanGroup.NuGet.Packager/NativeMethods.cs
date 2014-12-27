using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace OvermanGroup.NuGet.Packager
{
	[SecurityCritical]
	[SuppressUnmanagedCodeSecurity]
	internal static class NativeMethods
	{
		[DllImport(Constants.Kernel32, SetLastError = true, CharSet = CharSet.Unicode)]
		public static extern int SearchPath(
			[In] string lpPath,
			[In] string lpFileName,
			[In] string lpExtension,
			[In] int nBufferLength,
			[In, Out] StringBuilder lpBuffer,
			[Out] out IntPtr lpFilePart);

	}
}