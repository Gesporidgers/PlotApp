using System;
using System.Runtime.InteropServices;
using System.Text;

public static class SvgClipboardHelper
{
	[DllImport("user32.dll", SetLastError = true)]
	private static extern bool OpenClipboard(IntPtr hWndNewOwner);

	[DllImport("user32.dll", SetLastError = true)]
	private static extern bool CloseClipboard();

	[DllImport("user32.dll", SetLastError = true)]
	private static extern bool EmptyClipboard();

	[DllImport("user32.dll", SetLastError = true)]
	private static extern IntPtr SetClipboardData(uint uFormat, IntPtr hMem);

	[DllImport("user32.dll", SetLastError = true)]
	private static extern uint RegisterClipboardFormat(string lpszFormat);

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern IntPtr GlobalAlloc(uint uFlags, UIntPtr dwBytes);

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern IntPtr GlobalLock(IntPtr hMem);

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern bool GlobalUnlock(IntPtr hMem);

	private const uint GMEM_MOVEABLE = 0x0002;

	public static void SetSvg(string svgText)
	{
		// Регистрируем формат image/svg+xml
		uint svgFormat = RegisterClipboardFormat("image/svg+xml");
		if (svgFormat == 0)
			throw new Exception("Не удалось зарегистрировать формат буфера обмена.");

		// Кодируем SVG в UTF-8 (Word понимает такой вариант)
		byte[] bytes = Encoding.UTF8.GetBytes(svgText);

		// Выделяем глобальную память под строку + нулевой байт
		IntPtr hGlobal = GlobalAlloc(GMEM_MOVEABLE, (UIntPtr)(bytes.Length + 1));
		if (hGlobal == IntPtr.Zero)
			throw new Exception("Не удалось выделить память.");

		IntPtr target = GlobalLock(hGlobal);
		if (target == IntPtr.Zero)
			throw new Exception("Не удалось заблокировать память.");

		try
		{
			Marshal.Copy(bytes, 0, target, bytes.Length);
			Marshal.WriteByte(target, bytes.Length, 0); // завершающий \0
		}
		finally
		{
			GlobalUnlock(hGlobal);
		}

		if (!OpenClipboard(IntPtr.Zero))
			throw new Exception("Не удалось открыть буфер обмена.");

		try
		{
			EmptyClipboard();

			if (SetClipboardData(svgFormat, hGlobal) == IntPtr.Zero)
				throw new Exception("Не удалось поместить данные в буфер обмена.");

			// Важно: теперь памятью владеет система, освобождать вручную нельзя!
			hGlobal = IntPtr.Zero;
		}
		finally
		{
			CloseClipboard();
		}
	}
}