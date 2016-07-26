// Eval avoidence hacks by Chrisbot6

// Let's address some misconceptions about what we're doing here.
// 1. The following is actual valid code. This is regardless of any use of namespaces (as long as they are always valid (::) namespace usage) or numbers after the first character of the variable.
// 2. The following only needs to address letters and the underscore character - and also capital letters for safety - as they are all global variables are actually allowed to start with
// 3. Yes, you can probably optimize it even more by removing the capital letter entries - I don't know if they're needed, I literally put them there just in case.
// 4. No, eval isn't faster. In fact, eval is almost always slower than this method.

// setGlobalByName safely sets a global variable given the string that defines it.
// --------------------------------------
function setGlobalByName(%global, %value) {
	%firstLetter = getSubStr(%global, 0, 1);
	%theRest = getSubStr(%global, 1, strLen(%global)-1);
	
	if(%firstLetter $= "$") {
		return setGlobalByName(%theRest, %value);
	}
	
	// IT CAN BE DONE WITHOUT EVAL GOD DAMN IT
	
	switch$ (%firstLetter)
	{
		case "a":
			$a[%theRest] = %value;
		case "b":
			$b[%theRest] = %value;
		case "c":
			$c[%theRest] = %value;
		case "d":
			$d[%theRest] = %value;
		case "e":
			$e[%theRest] = %value;
		case "f":
			$f[%theRest] = %value;
		case "g":
			$g[%theRest] = %value;
		case "h":
			$h[%theRest] = %value;
		case "i":
			$i[%theRest] = %value;
		case "j":
			$j[%theRest] = %value;
		case "k":
			$k[%theRest] = %value;
		case "l":
			$l[%theRest] = %value;
		case "m":
			$m[%theRest] = %value;
		case "n":
			$n[%theRest] = %value;
		case "o":
			$o[%theRest] = %value;
		case "p":
			$p[%theRest] = %value;
		case "q":
			$q[%theRest] = %value;
		case "r":
			$r[%theRest] = %value;
		case "s":
			$s[%theRest] = %value;
		case "t":
			$t[%theRest] = %value;
		case "u":
			$u[%theRest] = %value;
		case "v":
			$v[%theRest] = %value;
		case "w":
			$w[%theRest] = %value;
		case "x":
			$x[%theRest] = %value;
		case "y":
			$y[%theRest] = %value;
		case "z":
			$z[%theRest] = %value;
		case "A":
			$A[%theRest] = %value;
		case "B":
			$B[%theRest] = %value;
		case "C":
			$C[%theRest] = %value;
		case "D":
			$D[%theRest] = %value;
		case "E":
			$E[%theRest] = %value;
		case "F":
			$F[%theRest] = %value;
		case "G":
			$G[%theRest] = %value;
		case "H":
			$H[%theRest] = %value;
		case "I":
			$I[%theRest] = %value;
		case "J":
			$J[%theRest] = %value;
		case "K":
			$K[%theRest] = %value;
		case "L":
			$L[%theRest] = %value;
		case "M":
			$M[%theRest] = %value;
		case "N":
			$N[%theRest] = %value;
		case "O":
			$O[%theRest] = %value;
		case "P":
			$P[%theRest] = %value;
		case "Q":
			$Q[%theRest] = %value;
		case "R":
			$R[%theRest] = %value;
		case "S":
			$S[%theRest] = %value;
		case "T":
			$T[%theRest] = %value;
		case "U":
			$U[%theRest] = %value;
		case "V":
			$V[%theRest] = %value;
		case "W":
			$W[%theRest] = %value;
		case "X":
			$X[%theRest] = %value;
		case "Y":
			$Y[%theRest] = %value;
		case "Z":
			$Z[%theRest] = %value;
		case "_":
			$_[%theRest] = %value;
	}
}

// getGlobalByName gives you the value of any global.
// --------------------------------------
function getGlobalByName(%global) {
	%firstLetter = getSubStr(%global, 0, 1);
	%theRest = getSubStr(%global, 1, strLen(%global)-1);
	
	if(%firstLetter $= "$") {
		return getGlobalByName(%theRest);
	}
	
	// IT CAN BE DONE WITHOUT EVAL GOD DAMN IT
	
	switch$ (%firstLetter)
	{
		case "a":
			return $a[%theRest];
		case "b":
			return $b[%theRest];
		case "c":
			return $c[%theRest];
		case "d":
			return $d[%theRest];
		case "e":
			return $e[%theRest];
		case "f":
			return $f[%theRest];
		case "g":
			return $g[%theRest];
		case "h":
			return $h[%theRest];
		case "i":
			return $i[%theRest];
		case "j":
			return $j[%theRest];
		case "k":
			return $k[%theRest];
		case "l":
			return $l[%theRest];
		case "m":
			return $m[%theRest];
		case "n":
			return $n[%theRest];
		case "o":
			return $o[%theRest];
		case "p":
			return $p[%theRest];
		case "q":
			return $q[%theRest];
		case "r":
			return $r[%theRest];
		case "s":
			return $s[%theRest];
		case "t":
			return $t[%theRest];
		case "u":
			return $u[%theRest];
		case "v":
			return $v[%theRest];
		case "w":
			return $w[%theRest];
		case "x":
			return $x[%theRest];
		case "y":
			return $y[%theRest];
		case "z":
			return $z[%theRest];
		case "A":
			return $A[%theRest];
		case "B":
			return $B[%theRest];
		case "C":
			return $C[%theRest];
		case "D":
			return $D[%theRest];
		case "E":
			return $E[%theRest];
		case "F":
			return $F[%theRest];
		case "G":
			return $G[%theRest];
		case "H":
			return $H[%theRest];
		case "I":
			return $I[%theRest];
		case "J":
			return $J[%theRest];
		case "K":
			return $K[%theRest];
		case "L":
			return $L[%theRest];
		case "M":
			return $M[%theRest];
		case "N":
			return $N[%theRest];
		case "O":
			return $O[%theRest];
		case "P":
			return $P[%theRest];
		case "Q":
			return $Q[%theRest];
		case "R":
			return $R[%theRest];
		case "S":
			return $S[%theRest];
		case "T":
			return $T[%theRest];
		case "U":
			return $U[%theRest];
		case "V":
			return $V[%theRest];
		case "W":
			return $W[%theRest];
		case "X":
			return $X[%theRest];
		case "Y":
			return $Y[%theRest];
		case "Z":
			return $Z[%theRest];
		case "_":
			return $_[%theRest];
	}
}