The ````Tags```` block maps all ASCII characters from U+0020 to U+007E to an invisible ````TAG```` equivalent between U+E0020 and U+E007E.
When composed together, ````TAG```` characters form a tag sequence, which is always terminated by the [U+E007F](/codepoints/E007F) ````CANCEL TAG```` character.

As of Unicode 10.0, the only valid use of tag sequences, is for representing flags of regions, when following the [U+1F3F4](/codepoints/1F3F4) ````WAVING BLACK FLAG```` (🏴) character.

e.g. To represent the flag of England, you would use [U+1F3F4](/codepoints/1F3F4) ````WAVING BLACK FLAG```` (🏴), followed by a sequence of ````TAG```` characters such as ````gbeng````, then by [U+E007F](/codepoints/E007F) ````CANCEL TAG````.
This would give this character: ````🏴󠁧󠁢󠁥󠁮󠁧󠁿````.
