using System.Drawing;
using System.Drawing.Imaging;
using Windows.Storage.Streams;
bool[] ToBin(byte number) {
	bool[] output = new bool[8];
	string h = Convert.ToString(number, 2).PadLeft(8, '0');
	for (int i = 0; i < 8; i++)
		output[i] = h[i] == '1';
	return output;
}
byte FromBin(bool[] bits) {
	byte output = 0;
	for (int i = bits.Length - 1; i >= 0; i--) {
		if (bits[i])
			output += (byte)Math.Pow(2, 7 - i);
	}
	return output;
}
void Invert(Bitmap b, bool vertical = false) {
	for (int x = 0; x < b.Width; x++) {
		if (!vertical && x % 2 == 0)
			continue;
		for (int y = 0; y < b.Height - 2; y++) {
			if (vertical && y % 2 == 0)
				continue;
			bool pixel = b.GetPixel(x, y).R == 0;
			b.SetPixel(x, y, pixel ? Color.White : Color.Black);
		}
	}
}
string? whatToDo = "";
while (whatToDo != "encode" && whatToDo != "decode") {
	Console.Write("encode/decode:");
	whatToDo = Console.ReadLine();
}
if (whatToDo == "encode") {
	Console.ResetColor();
	Console.WriteLine("Enter the text you want to encode (max 120 chars):");
	string? text = null;
	bool read = false;
	while (text == null) {
		text = Console.ReadLine();
		if (text == "=!read") {
			read = true;
			break;
		}
		if (text == null) {
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine("Enter somethigg");
			Console.ResetColor();
		} else if (text.Length > 120) {
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine("Too many characters");
			Console.ResetColor();
			text = null;
		}
	}
	byte[] chars = new byte[text.Length];
	Console.WriteLine("Converting to bytes...");
	for (int i = 0; i < text.Length; i++) {
		if (char.IsAscii(text[i])) {
			chars[i] = (byte)text[i];
		} else {
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine($"character {text[i]} is not in ASCII");
			Console.ResetColor();
		}
	}
	Console.WriteLine();
	Bitmap bmp = new(32, 32);
	using Graphics g = Graphics.FromImage(bmp);
	g.Clear(Color.White);
	Pen pen = new(new SolidBrush(Color.Black));
	g.DrawLine(pen, new Point(0, 31), new Point(31, 31));
	int y = 0;
	bool[] lenBits = ToBin((byte)text.Length);
	for (int i = 0; i < lenBits.Length; i++) {
		if (lenBits[i] == true)
			bmp.SetPixel(i, 32 - 2, Color.Black);
	}
	Console.ForegroundColor = ConsoleColor.Black;
	Console.BackgroundColor = ConsoleColor.White;
	for (int i = 0; i < chars.Length; i++) {
		int x = (i * 8) % 32;
		bool[] bits = ToBin(chars[i]);
		for (int j = 0; j < bits.Length; j++) {
			if (bits[j])
				bmp.SetPixel(x + j, y, Color.Black);
		}
		if (x == 24) {
			y++;
		}
	}
	Invert(bmp, true);
	Invert(bmp);
	Console.WriteLine();
	Console.ResetColor();
	bmp.Save("code.png", ImageFormat.Png);
	Console.WriteLine($"Saved code as {Path.Combine(Environment.CurrentDirectory, "code.png")}");
	Console.ReadKey();
} else if (whatToDo == "decode") {
	Bitmap bmp = new Bitmap("code.png");
	Invert(bmp);
	Invert(bmp, true);
	int length = 0;
	for (int i = 0; i < 8; i++) {
		if (bmp.GetPixel(7 - i, 30).R == 0)
			length += (int)Math.Pow(2, i);
	}
	byte[] chars = new byte[length];
	string data = "";
	int y = 0;
	for (int i = 0; i < length; i++) {
		int x = (i * 8) % 32;
		bool[] bits = new bool[8];
		for (int j = 0; j < 8; j++) {
			Console.Title = $"{x + j}, {y}";
			bool bit = bmp.GetPixel(x + j, y).R < 128;
			bits[j] = bit;
		}
		chars[i] = FromBin(bits);
		data += (char)chars[i];
		if (x >= 24) 
			y++;
		
	}
	Console.WriteLine();
	Console.WriteLine(data);
	Console.ReadKey();
}
