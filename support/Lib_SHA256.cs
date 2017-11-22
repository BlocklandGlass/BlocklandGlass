// modified to use global variables instead

$sha256_ASCIITable="\x01\x02\x03\x04\x05\x06\x07\x08\x09\x0A\x0B\x0C\x0D\x0E\x0F"@
	"\x10\x11\x12\x13\x14\x15\x16\x17\x18\x19\x1A\x1B\x1C\x1D\x1E\x1F"@
	"\x20\x21\x22\x23\x24\x25\x26\x27\x28\x29\x2A\x2B\x2C\x2D\x2E\x2F"@
	"\x30\x31\x32\x33\x34\x35\x36\x37\x38\x39\x3A\x3B\x3C\x3D\x3E\x3F"@
	"\x40\x41\x42\x43\x44\x45\x46\x47\x48\x49\x4A\x4B\x4C\x4D\x4E\x4F"@
	"\x50\x51\x52\x53\x54\x55\x56\x57\x58\x59\x5A\x5B\x5C\x5D\x5E\x5F"@
	"\x60\x61\x62\x63\x64\x65\x66\x67\x68\x69\x6A\x6B\x6C\x6D\x6E\x6F"@
	"\x70\x71\x72\x73\x74\x75\x76\x77\x78\x79\x7A\x7B\x7C\x7D\x7E\x7F"@
	"\x80\x81\x82\x83\x84\x85\x86\x87\x88\x89\x8A\x8B\x8C\x8D\x8E\x8F"@
	"\x90\x91\x92\x93\x94\x95\x96\x97\x98\x99\x9A\x9B\x9C\x9D\x9E\x9F"@
	"\xA0\xA1\xA2\xA3\xA4\xA5\xA6\xA7\xA8\xA9\xAA\xAB\xAC\xAD\xAE\xAF"@
	"\xB0\xB1\xB2\xB3\xB4\xB5\xB6\xB7\xB8\xB9\xBA\xBB\xBC\xBD\xBE\xBF"@
	"\xC0\xC1\xC2\xC3\xC4\xC5\xC6\xC7\xC8\xC9\xCA\xCB\xCC\xCD\xCE\xCF"@
	"\xD0\xD1\xD2\xD3\xD4\xD5\xD6\xD7\xD8\xD9\xDA\xDB\xDC\xDD\xDE\xDF"@
	"\xE0\xE1\xE2\xE3\xE4\xE5\xE6\xE7\xE8\xE9\xEA\xEB\xEC\xED\xEE\xEF"@
	"\xF0\xF1\xF2\xF3\xF4\xF5\xF6\xF7\xF8\xF9\xFA\xFB\xFC\xFD\xFE\xFF";

// implemented using pseudocode from https://en.wikipedia.org/wiki/SHA-2#Pseudocode
// inline variant of Port's add function from https://forum.blockland.us/index.php?topic=248922.0
// tested against SHA256 test vectors from http://www.di-mgt.com.au/sha_testvectors.html
function sha256($sha256_text)
{
  $sha256_o = "";
	// initialize hash values:
	// first 32 bits of the fractional parts of the square roots of the first 8 primes (2 through 19):
	$sha256_h0 = 0x6A09E667;
	$sha256_h1 = 0xBB67AE85;
	$sha256_h2 = 0x3C6EF372;
	$sha256_h3 = 0xA54FF53A;
	$sha256_h4 = 0x510E527F;
	$sha256_h5 = 0x9B05688C;
	$sha256_h6 = 0x1F83D9AB;
	$sha256_h7 = 0x5BE0CD19;

	// initialize array of round constants:
	// first 32 bits of the fractional parts of the cube roots of the first 64 primes (2 through 311):
	$sha256_k[0] = 0x428A2F98; $sha256_k[1] = 0x71374491; $sha256_k[2] = 0xB5C0FBCF; $sha256_k[3] = 0xE9B5DBA5;
	$sha256_k[4] = 0x3956C25B; $sha256_k[5] = 0x59F111F1; $sha256_k[6] = 0x923F82A4; $sha256_k[7] = 0xAB1C5ED5;
	$sha256_k[8] = 0xD807AA98; $sha256_k[9] = 0x12835B01; $sha256_k[10] = 0x243185BE; $sha256_k[11] = 0x550C7DC3;
	$sha256_k[12] = 0x72BE5D74; $sha256_k[13] = 0x80DEB1FE; $sha256_k[14] = 0x9BDC06A7; $sha256_k[15] = 0xC19BF174;
	$sha256_k[16] = 0xE49B69C1; $sha256_k[17] = 0xEFBE4786; $sha256_k[18] = 0x0FC19DC6; $sha256_k[19] = 0x240CA1CC;
	$sha256_k[20] = 0x2DE92C6F; $sha256_k[21] = 0x4A7484AA; $sha256_k[22] = 0x5CB0A9DC; $sha256_k[23] = 0x76F988DA;
	$sha256_k[24] = 0x983E5152; $sha256_k[25] = 0xA831C66D; $sha256_k[26] = 0xB00327C8; $sha256_k[27] = 0xBF597FC7;
	$sha256_k[28] = 0xC6E00BF3; $sha256_k[29] = 0xD5A79147; $sha256_k[30] = 0x06CA6351; $sha256_k[31] = 0x14292967;
	$sha256_k[32] = 0x27B70A85; $sha256_k[33] = 0x2E1B2138; $sha256_k[34] = 0x4D2C6DFC; $sha256_k[35] = 0x53380D13;
	$sha256_k[36] = 0x650A7354; $sha256_k[37] = 0x766A0ABB; $sha256_k[38] = 0x81C2C92E; $sha256_k[39] = 0x92722C85;
	$sha256_k[40] = 0xA2BFE8A1; $sha256_k[41] = 0xA81A664B; $sha256_k[42] = 0xC24B8B70; $sha256_k[43] = 0xC76C51A3;
	$sha256_k[44] = 0xD192E819; $sha256_k[45] = 0xD6990624; $sha256_k[46] = 0xF40E3585; $sha256_k[47] = 0x106AA070;
	$sha256_k[48] = 0x19A4C116; $sha256_k[49] = 0x1E376C08; $sha256_k[50] = 0x2748774C; $sha256_k[51] = 0x34B0BCB5;
	$sha256_k[52] = 0x391C0CB3; $sha256_k[53] = 0x4ED8AA4A; $sha256_k[54] = 0x5B9CCA4F; $sha256_k[55] = 0x682E6FF3;
	$sha256_k[56] = 0x748F82EE; $sha256_k[57] = 0x78A5636F; $sha256_k[58] = 0x84C87814; $sha256_k[59] = 0x8CC70208;
	$sha256_k[60] = 0x90BEFFFA; $sha256_k[61] = 0xA4506CEB; $sha256_k[62] = 0xBEF9A3F7; $sha256_k[63] = 0xC67178F2;

	// pre-processing:
	$sha256_len = strLen($sha256_text);
	for ($sha256_i = 0; $sha256_i < $sha256_len; $sha256_i++)
		$sha256_byte[$sha256_i] = strPos($sha256_ASCIITable, getSubStr($sha256_text, $sha256_i, 1)) + 1;

	// append a single 1 bit to the end of the original message,
	// then 0 bits to pad the message to 64 bits less than a full chunk
	$sha256_byte[$sha256_len] = 128;
	$sha256_lPos = ($sha256_len + 8 | 63) - 7 | 0;
	for ($sha256_i = $sha256_len + 1; $sha256_i < $sha256_lPos; $sha256_i++)
		$sha256_byte[$sha256_i] = 0;

	// append the length of the original message, in bits, to the end of the message
	// the length is appended as a 64-bit big-endian integer
	$sha256_bitLen = $sha256_len << 3;
	for ($sha256_i = 0; $sha256_i < 8; $sha256_i++)
	{
		$sha256_byte[$sha256_lPos + 7 - $sha256_i] = $sha256_bitLen & 255;
		$sha256_bitLen >>= 8;
	}
	$sha256_len = $sha256_lPos + 8 | 0;

	// convert the bytes to 32-bit words
	$sha256_wLen = $sha256_len >> 2;
	for ($sha256_i = 0; $sha256_i < $sha256_wLen; $sha256_i++)
	{
		$sha256_bPos = $sha256_i << 2;
		for ($sha256_j = 0; $sha256_j < 4; $sha256_j++)
			$sha256_word[$sha256_i] = $sha256_word[$sha256_i] << 8 | $sha256_byte[$sha256_bPos + $sha256_j];
	}

	// process the message in 512-bit chunks (512 / 32 = 16, so $sha256_wLen >> 4):
	$sha256_chunks = $sha256_wLen >> 4;
	for ($sha256_chunk = 0; $sha256_chunk < $sha256_chunks; $sha256_chunk++)
	{
		// copy the current chunk to the beginning of the message schedule array
		$sha256_offset = $sha256_chunk << 4;
		for ($sha256_i = 0; $sha256_i < 16; $sha256_i++)
			$sha256_w[$sha256_i] = $sha256_word[$sha256_i + $sha256_offset];

		// extend the first 16 words into the remaining 48 words
		for ($sha256_i = 16; $sha256_i < 64; $sha256_i++)
		{
			$sha256_s0 = ($sha256_w[$sha256_i - 15] >> 7 | $sha256_w[$sha256_i - 15] << 25) ^ ($sha256_w[$sha256_i - 15] >> 18 | $sha256_w[$sha256_i - 15] << 14) ^ $sha256_w[$sha256_i - 15] >> 3;
			$sha256_s1 = ($sha256_w[$sha256_i - 2] >> 17 | $sha256_w[$sha256_i - 2] << 15) ^ ($sha256_w[$sha256_i - 2] >> 19 | $sha256_w[$sha256_i - 2] << 13) ^ $sha256_w[$sha256_i - 2] >> 10;

			// inline version of Port's add function: https://forum.blockland.us/index.php?topic=248922.0
			// We have to avoid native addition for this because TorqueScript cannot do math. :(
			// Operation: $sha256_w[$sha256_i] = $sha256_w[$sha256_i - 16] + $sha256_s0 + $sha256_w[$sha256_i - 7] + $sha256_s1;
			$sha256_add0 = $sha256_w[$sha256_i - 16];
			$sha256_add1 = $sha256_s0;
			$sha256_add2 = $sha256_w[$sha256_i - 7];
			$sha256_add3 = $sha256_s1;
			for ($sha256_j = 0; $sha256_j < 3; $sha256_j++)
			{
				$sha256__a = 1;
				$sha256__x = $sha256_add[$sha256_j];
				$sha256__y = $sha256_add[$sha256_j + 1];
				while ($sha256__a)
				{
					$sha256__a = $sha256__x & $sha256__y;
					$sha256__b = $sha256__x ^ $sha256__y;
					$sha256__x = $sha256__a << 1;
					$sha256__y = $sha256__b;
				}
				$sha256_add[$sha256_j + 1] = $sha256__b;
			}
			$sha256_w[$sha256_i] = $sha256__b;
		}

		// initialize working variables
		$sha256_a = $sha256_h0;
		$sha256_b = $sha256_h1;
		$sha256_c = $sha256_h2;
		$sha256_d = $sha256_h3;
		$sha256_e = $sha256_h4;
		$sha256_f = $sha256_h5;
		$sha256_g = $sha256_h6;
		$sha256_h = $sha256_h7;

		// main compression function
		// the "$sha256_i < 64" here controls the number of rounds
		for ($sha256_i = 0; $sha256_i < 64; $sha256_i++)
		{
			$sha256_s1 = ($sha256_e >> 6 | $sha256_e << 26) ^ ($sha256_e >> 11 | $sha256_e << 21) ^ ($sha256_e >> 25 | $sha256_e << 7);
			$sha256_ch = ($sha256_e & $sha256_f) ^ (~$sha256_e & $sha256_g);

			// inline Port add
			// Operation: $sha256_temp1 = $sha256_h + $sha256_s1 + $sha256_ch + $sha256_k[$sha256_i] + $sha256_w[$sha256_i]
			$sha256_add0 = $sha256_h;
			$sha256_add1 = $sha256_s1;
			$sha256_add2 = $sha256_ch;
			$sha256_add3 = $sha256_k[$sha256_i];
			$sha256_add4 = $sha256_w[$sha256_i];
			for ($sha256_j = 0; $sha256_j < 4; $sha256_j++)
			{
				$sha256__a = 1;
				$sha256__x = $sha256_add[$sha256_j];
				$sha256__y = $sha256_add[$sha256_j + 1];
				while ($sha256__a)
				{
					$sha256__a = $sha256__x & $sha256__y;
					$sha256__b = $sha256__x ^ $sha256__y;
					$sha256__x = $sha256__a << 1;
					$sha256__y = $sha256__b;
				}
				$sha256_add[$sha256_j + 1] = $sha256__b;
			}
			$sha256_temp1 = $sha256__b;
			$sha256_s0 = ($sha256_a >> 2 | $sha256_a << 30) ^ ($sha256_a >> 13 | $sha256_a << 19) ^ ($sha256_a >> 22 | $sha256_a << 10);
			$sha256_maj = ($sha256_a & $sha256_b) ^ ($sha256_a & $sha256_c) ^ ($sha256_b & $sha256_c);

			// inline Port add
			// Operation: $sha256_temp2 = $sha256_s0 + $sha256_maj;
			$sha256__a = 1;
			while ($sha256__a)
			{
				$sha256__a = $sha256_s0 & $sha256_maj;
				$sha256__b = $sha256_s0 ^ $sha256_maj;
				$sha256_s0 = $sha256__a << 1;
				$sha256_maj = $sha256__b;
			}
			$sha256_temp2 = $sha256__b;

			$sha256_h = $sha256_g;
			$sha256_g = $sha256_f;
			$sha256_f = $sha256_e;

			// inline Port add
			// Operation: $sha256_e = $sha256_d + $sha256_temp1;
			$sha256__a = 1;
			$sha256__x = $sha256_temp1;
			while ($sha256__a)
			{
				$sha256__a = $sha256__x & $sha256_d;
				$sha256__b = $sha256__x ^ $sha256_d;
				$sha256__x = $sha256__a << 1;
				$sha256_d = $sha256__b;
			}
			$sha256_e = $sha256__b;
			$sha256_d = $sha256_c;
			$sha256_c = $sha256_b;
			$sha256_b = $sha256_a;

			// inline Port add
			// Operation: $sha256_a = $sha256_temp1 + $sha256_temp2;
			$sha256__a = 1;
			while ($sha256__a)
			{
				$sha256__a = $sha256_temp1 & $sha256_temp2;
				$sha256__b = $sha256_temp1 ^ $sha256_temp2;
				$sha256_temp1 = $sha256__a << 1;
				$sha256_temp2 = $sha256__b;
			}
			$sha256_a = $sha256__b;
		}

		// increment $sha256_h[0] through $sha256_h[7] by $sha256_a through $sha256_h
		// we can't do this more simply by using $sha256_h[0] += $sha256_a; because TorqueScript cannot add
		$sha256_v0 = $sha256_a; $sha256_v1 = $sha256_b; $sha256_v2 = $sha256_c; $sha256_v3 = $sha256_d;
		$sha256_v4 = $sha256_e; $sha256_v5 = $sha256_f; $sha256_v6 = $sha256_g; $sha256_v7 = $sha256_h;
		for ($sha256_i = 0; $sha256_i < 8; $sha256_i++)
		{
			// inline Port add
			// Operation: $sha256_h[$sha256_i] = $sha256_h[$sha256_i] + $sha256_v[$sha256_i];
			$sha256__a = 1;
			$sha256__x = $sha256_h[$sha256_i];
			$sha256__y = $sha256_v[$sha256_i];
			while ($sha256__a)
			{
				$sha256__a = $sha256__x & $sha256__y;
				$sha256__b = $sha256__x ^ $sha256__y;
				$sha256__x = $sha256__a << 1;
				$sha256__y = $sha256__b;
			}
			$sha256_h[$sha256_i] = $sha256__b;
		}
	}

	// produce hexadecimal string and return it
	$sha256_hex = "0123456789abcdef";
	for ($sha256_i = 0; $sha256_i < 8; $sha256_i++)
	{
		$sha256_word = "";
		for ($sha256_j = 0; $sha256_j < 8; $sha256_j++)
		{
			$sha256_word = getSubStr($sha256_hex, $sha256_h[$sha256_i] & 15, 1) @ $sha256_word;
			$sha256_h[$sha256_i] >>= 4;
		}
		$sha256_o = $sha256_o @ $sha256_word;
	}
	return $sha256_o;
}
