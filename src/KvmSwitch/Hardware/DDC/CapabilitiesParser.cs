namespace KvmSwitch.Hardware.DDC
{
    using System.Collections.Generic;
    using System.Linq;

    internal static class CapabilitiesParser
    {
        public static DisplayCapabilities ParseToDisplayCapabilities(string input)
        {
            return new DisplayCapabilities(ParseCapabilities(input));
        }

        public static List<CapabilityTag> ParseCapabilities(string input)
        {
            // 1.	The terminal symbols of the grammar are STRING, TAG, WS, `(', and `)'.
            // 2.	WS is a sequence of one or more white space characters: SPACE, TAB, RETURN or  LF.
            // 3.   A STRING is a sequence of one or more non - white space characters.
            //      All Capabilities data shall be represented as STRING: integers(123), floats(+3.0e8), strings(keyboard).
            //      Special characters may be included in STRINGs by escaping them as \xHH where HH represents two hexadecimal digits.
            //      SPACE, TAB, RETURN, LF, `(', `)', and `\' are special characters and must be escaped as \xHH to include them in STRINGs.
            // 4.   A TAG is a STRING which is immediately followed by a `('.
            // 5.  `(' and `)' are open and close parenthesis used for grouping.


            var capabilities = new List<CapabilityTag>();

            if (string.IsNullOrEmpty(input))
            {
                return capabilities;
            }

            var stack = new List<CapabilityTag>();
            var lastNonWhitespaceChar = '\0';

            for (var i = 0; i < input.Length; i++)
            {
                var c = input[i];

                switch (c)
                {
                    case '(':
                        stack.Add(new CapabilityTag());
                        break;
                    case ')':
                        {
                            if (stack.Count == 0)
                            {
                                break; // Last closing bracket
                            }

                            // A(1 2)
                            // removes child from stack
                            RemoveLastItemFromStack(stack, capabilities);
                            // removes tag from stack
                            RemoveLastItemFromStack(stack, capabilities);

                            break;
                        }

                    // WS is a sequence of one or more white space characters
                    case ' ':
                    case '\t':
                    case '\r':
                    case '\n':
                        {
                            // ignore if next char is white space too
                            if (i + 1 < input.Length && IsWhiteSpaceCharacter(input[i + 1]))
                            {
                                break;
                            }

                            // tag can be finished by ')' or ' '
                            // ") " should not close tag again

                            if (lastNonWhitespaceChar == ')')
                            {
                                stack.Add(new CapabilityTag());
                                break;
                            }

                            var last = stack.Last();

                            stack.Remove(last);

                            if (stack.Count == 0)
                            {
                                AddItemIfNotEmpty(last, capabilities);
                            }
                            else
                            {
                                stack.Last().Tags.Add(last);
                                // ignore whitespace if next char is ")"
                                if (c == ' ' && i + 1 < input.Length && input[i + 1] == ')')
                                {
                                    break;
                                }

                                stack.Add(new CapabilityTag());
                            }

                            break;
                        }
                    default:
                        if (stack.Count == 0)
                        {
                            stack.Add(new CapabilityTag());
                        }

                        stack.Last().Name += c;
                        break;
                }

                if (!IsWhiteSpaceCharacter(c))
                {
                    lastNonWhitespaceChar = c;
                }
            }

            return capabilities;
        }


        private static bool IsWhiteSpaceCharacter(char value)
        {
            return value == ' ' || value == '\t' || value == '\r' || value == '\n';
        }

        private static void AddItemIfNotEmpty(CapabilityTag capabilityTag, List<CapabilityTag> capabilities)
        {
            if (!string.IsNullOrEmpty(capabilityTag.Name))
            {
                capabilities.Add(capabilityTag);
            }
        }

        private static void RemoveLastItemFromStack(ICollection<CapabilityTag> stack, List<CapabilityTag> capabilities)
        {
            if (stack.Count == 0)
            {
                return;
            }

            var last = stack.Last();

            if (last == null)
            {
                return;
            }

            stack.Remove(last);

            if (stack.Count == 0)
            {
                AddItemIfNotEmpty(last, capabilities);
            }
            else
            {
                stack.Last().Tags.Add(last);
            }
        }
    }
}
