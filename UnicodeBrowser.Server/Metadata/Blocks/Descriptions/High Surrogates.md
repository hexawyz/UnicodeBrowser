The ````High Surrogates```` block contains code points that are reserved for representing the high part of surrogate pairs.

Surrogate pairs are composed of a high surrogate and a low surrogate, and are exclusively used in the UTF-16 encoding to represent any code point between U+10000 and U+1FFFFF.

To get the code point represented by a surrogate pair ````(uint16 high, uint16 low)````, the following formula can be applied:
````0x10000 + ((high - 0xD800) << 10) + (low - 0xDC00)````
