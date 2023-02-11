using System.Collections.Generic;
using System.Linq;
using LunarLander.geometry2d;

namespace LunarLander.data; 

public class VectorText {
    public IEnumerable<Line> Lines { get; private set; }
    public Point position { get; private set; }
    public double scale { get; private set; }
    private string text;

    public VectorText(string text, Point position, double scale) {
        this.position = position;
        this.scale = scale;
        setText(text);
    }

    public void setText(string text) {
        if (this.text == text) {
            return;
        }
        this.text = text;
        int x = 0;
        int y = 0;
        Lines = new List<Line>();
        foreach (char c in text) {
            int index = c;
            switch (index) {
                case 10: // 10 = \n
                    x = 0;
                    y++;
                    break;
                case >= 32 and <= 126:
                    Lines = Lines.Concat(ascii[index].Select(l => {
                        Line cl = l.clone();
                        cl.scale(scale * 0.8);
                        cl.translate(x * scale, 2 * y * scale);
                        cl.translate(position);
                        return cl;
                    }));
                    x++;
                    break;
                default:
                    throw new System.ArgumentException("Invalid character: " + c);
            }
        }
    }
    
    public void translate(Point p) {
        translate(p.x, p.y);
    }
    
    public void translate(double x, double y) {
        foreach (Line s in Lines) {
            s.translate(x, y);
        }
    }

    private static readonly List<List<Line>> ascii = new (new[] {
        /* Control characters */
        new List<Line>(), // 0x00
        new List<Line>(), // 0x01
        new List<Line>(), // 0x02
        new List<Line>(), // 0x03
        new List<Line>(), // 0x04
        new List<Line>(), // 0x05
        new List<Line>(), // 0x06
        new List<Line>(), // 0x07
        new List<Line>(), // 0x08
        new List<Line>(), // 0x09
        new List<Line>(), // 0x0A
        new List<Line>(), // 0x0B
        new List<Line>(), // 0x0C
        new List<Line>(), // 0x0D
        new List<Line>(), // 0x0E
        new List<Line>(), // 0x0F
        new List<Line>(), // 0x10
        new List<Line>(), // 0x11
        new List<Line>(), // 0x12
        new List<Line>(), // 0x13
        new List<Line>(), // 0x14
        new List<Line>(), // 0x15
        new List<Line>(), // 0x16
        new List<Line>(), // 0x17
        new List<Line>(), // 0x18
        new List<Line>(), // 0x19
        new List<Line>(), // 0x1A
        new List<Line>(), // 0x1B
        new List<Line>(), // 0x1C
        new List<Line>(), // 0x1D
        new List<Line>(), // 0x1E
        new List<Line>(), // 0x1F

        /* Space */ // 0x20
        new List<Line>(),

        /* ! */ // 0x21
        new List<Line>(new[] {
            new Line(new Point(0.5, 0.0), new Point(0.5, 1.4)),
            new Line(new Point(0.5, 1.7), new Point(0.5, 2.0))
        }),

        /* " */ // 0x22
        new List<Line>(new[] {
            new Line(new Point(0.3, 0.0), new Point(0.3, 0.5)),
            new Line(new Point(0.7, 0.0), new Point(0.7, 0.5))
        }),

        /* # */ // 0x23
        new List<Line>(new[] {
            new Line(new Point(0.2, 0.0), new Point(0.2, 2.0)),
            new Line(new Point(0.8, 0.0), new Point(0.8, 2.0)),
            new Line(new Point(0.0, 0.5), new Point(1.0, 0.5)),
            new Line(new Point(0.0, 1.5), new Point(1.0, 1.5))
        }),

        /* $ */ // 0x24
        new List<Line>(new[] {
            new Line(new Point(1.0, 0.2), new Point(0.0, 0.2)),
            new Line(new Point(0.0, 0.2), new Point(0.0, 0.8)),
            new Line(new Point(0.0, 0.8), new Point(1.0, 0.8)),
            new Line(new Point(1.0, 0.8), new Point(1.0, 1.8)),
            new Line(new Point(1.0, 1.8), new Point(0.0, 1.8)),
            new Line(new Point(0.5, 0.0), new Point(0.5, 2.0)),
        }),
        
        /* % */ // 0x25
        new List<Line>(new[] {
            new Line(new Point(0.0, 2.0), new Point(1.0, 0.0)),
            new Line(new Point(0.0, 0.0), new Point(0.2, 0.4)),
            new Line(new Point(0.8, 1.6), new Point(1.0, 2.0)),
        }),
        
        /* & */ // 0x26
        new List<Line>(new Line[]{
            // no.
        }),
        
        /* ' */ // 0x27
        new List<Line>(new[] {
            new Line(new Point(0.5, 0.0), new Point(0.5, 0.5)),
        }),
        
        /* ( */ // 0x28
        new List<Line>(new[] {
            new Line(new Point(0.8, 0.0), new Point(0.0, 0.5)),
            new Line(new Point(0.0, 0.5), new Point(0.0, 1.5)),
            new Line(new Point(0.0, 1.5), new Point(0.8, 2.0)),
        }),
        
        /* ) */ // 0x29
        new List<Line>(new[] {
            new Line(new Point(0.0, 0.0), new Point(0.8, 0.5)),
            new Line(new Point(0.8, 0.5), new Point(0.8, 1.5)),
            new Line(new Point(0.8, 1.5), new Point(0.0, 2.0)),
        }),
        
        /* * */ // 0x2A
        new List<Line>(new[] {
            new Line(new Point(0.2, 0.7), new Point(0.8, 1.3)),
            new Line(new Point(0.2, 1.3), new Point(0.8, 0.7)),
            new Line(new Point(0.2, 1.0), new Point(0.8, 1.0)),
        }),
        
        /* + */ // 0x2B
        new List<Line>(new[] {
            new Line(new Point(0.0, 1.0), new Point(1.0, 1.0)),
            new Line(new Point(0.5, 0.5), new Point(0.5, 1.5)),
        }),
        
        /* , */ // 0x2C
        new List<Line>(new[] {
            new Line(new Point(0.5, 1.8), new Point(0.3, 2.0)),
        }),
        
        /* - */ // 0x2D
        new List<Line>(new[] {
            new Line(new Point(0.0, 1.0), new Point(1.0, 1.0)),
        }),
        
        /* . */ // 0x2E
        new List<Line>(new[] {
            new Line(new Point(0.5, 1.8), new Point(0.5, 2.0)),
        }),
        
        /* / */ // 0x2F
        new List<Line>(new[] {
            new Line(new Point(0.0, 0.0), new Point(1.0, 2.0)),
        }),
        
        /* 0 */ // 0x30
        new List<Line>(new[] {
            new Line(new Point(0.0, 0.0), new Point(0.0, 2.0)),
            new Line(new Point(0.0, 2.0), new Point(1.0, 2.0)),
            new Line(new Point(1.0, 2.0), new Point(1.0, 0.0)),
            new Line(new Point(1.0, 0.0), new Point(0.0, 0.0)),
            new Line(new Point(1.0, 0.0), new Point(0.0, 2.0)),
        }),
        
        /* 1 */ // 0x31
        new List<Line>(new[] {
            new Line(new Point(0.5, 0.0), new Point(0.5, 2.0)),
            new Line(new Point(0.5, 0.0), new Point(0.0, 0.5)),
        }),
        
        /* 2 */ // 0x32
        new List<Line>(new[] {
            new Line(new Point(0.0, 0.0), new Point(1.0, 0.0)),
            new Line(new Point(1.0, 0.0), new Point(1.0, 1.0)),
            new Line(new Point(1.0, 1.0), new Point(0.0, 1.0)),
            new Line(new Point(0.0, 1.0), new Point(0.0, 2.0)),
            new Line(new Point(0.0, 2.0), new Point(1.0, 2.0)),
        }),
        
        /* 3 */ // 0x33
        new List<Line>(new[] {
            new Line(new Point(0.0, 0.0), new Point(1.0, 0.0)),
            new Line(new Point(1.0, 0.0), new Point(1.0, 2.0)),
            new Line(new Point(1.0, 2.0), new Point(0.0, 2.0)),
            new Line(new Point(0.0, 1.0), new Point(1.0, 1.0)),
        }),
        
        /* 4 */ // 0x34
        new List<Line>(new[] {
            new Line(new Point(0.0, 0.0), new Point(0.0, 1.0)),
            new Line(new Point(0.0, 1.0), new Point(1.0, 1.0)),
            new Line(new Point(1.0, 0.0), new Point(1.0, 2.0)),
        }),
        
        /* 5 */ // 0x35
        new List<Line>(new[] {
            new Line(new Point(1.0, 0.0), new Point(0.0, 0.0)),
            new Line(new Point(0.0, 0.0), new Point(0.0, 1.0)),
            new Line(new Point(0.0, 1.0), new Point(1.0, 1.0)),
            new Line(new Point(1.0, 1.0), new Point(1.0, 2.0)),
            new Line(new Point(1.0, 2.0), new Point(0.0, 2.0)),
        }),
        
        /* 6 */ // 0x36
        new List<Line>(new[] {
            new Line(new Point(1.0, 0.0), new Point(0.0, 0.0)),
            new Line(new Point(0.0, 0.0), new Point(0.0, 2.0)),
            new Line(new Point(0.0, 2.0), new Point(1.0, 2.0)),
            new Line(new Point(1.0, 2.0), new Point(1.0, 1.0)),
            new Line(new Point(1.0, 1.0), new Point(0.0, 1.0)),
        }),
        
        /* 7 */ // 0x37
        new List<Line>(new[] {
            new Line(new Point(0.0, 0.0), new Point(1.0, 0.0)),
            new Line(new Point(1.0, 0.0), new Point(1.0, 2.0)),
        }),
        
        /* 8 */ // 0x38
        new List<Line>(new[] {
            new Line(new Point(0.0, 0.0), new Point(0.0, 2.0)),
            new Line(new Point(0.0, 2.0), new Point(1.0, 2.0)),
            new Line(new Point(1.0, 2.0), new Point(1.0, 0.0)),
            new Line(new Point(1.0, 0.0), new Point(0.0, 0.0)),
            new Line(new Point(0.0, 1.0), new Point(1.0, 1.0)),
        }),
        
        /* 9 */ // 0x39
        new List<Line>(new[] {
            new Line(new Point(1.0, 0.0), new Point(1.0, 2.0)),
            new Line(new Point(0.0, 1.0), new Point(1.0, 1.0)),
            new Line(new Point(0.0, 1.0), new Point(0.0, 0.0)),
            new Line(new Point(0.0, 0.0), new Point(1.0, 0.0)),
        }),
        
        /* : */ // 0x3A
        new List<Line>(new[] {
            new Line(new Point(0.5, 0.3), new Point(0.5, 0.7)),
            new Line(new Point(0.5, 1.3), new Point(0.5, 1.7)),
        }),
        
        /* ; */ // 0x3B
        new List<Line>(new[] {
            new Line(new Point(0.5, 0.3), new Point(0.5, 0.7)),
            new Line(new Point(0.5, 1.4), new Point(0.4, 1.7)),
        }),
        
        /* < */ // 0x3C
        new List<Line>(new[] {
            new Line(new Point(1.0, 0.0), new Point(0.0, 1.0)),
            new Line(new Point(0.0, 1.0), new Point(1.0, 2.0)),
        }),
        
        /* = */ // 0x3D
        new List<Line>(new[] {
            new Line(new Point(0.0, 0.5), new Point(1.0, 0.5)),
            new Line(new Point(0.0, 1.5), new Point(1.0, 1.5)),
        }),
        
        /* > */ // 0x3E
        new List<Line>(new[] {
            new Line(new Point(0.0, 0.0), new Point(1.0, 1.0)),
            new Line(new Point(1.0, 1.0), new Point(0.0, 2.0)),
        }),
        
        /* ? */ // 0x3F
        new List<Line>(new[] {
            new Line(new Point(0.0, 0.0), new Point(1.0, 0.0)),
            new Line(new Point(1.0, 0.0), new Point(1.0, 1.0)),
            new Line(new Point(1.0, 1.0), new Point(0.5, 1.0)),
            new Line(new Point(0.5, 1.0), new Point(0.5, 1.5)),
        }),
        
        /* @ */ // 0x40
        new List<Line>(System.Array.Empty<Line>()),
        
        /* A */ // 0x41
        new List<Line>(new[] {
            new Line(new Point(0.5, 0.0), new Point(0.0, 2.0)),
            new Line(new Point(0.5, 0.0), new Point(1.0, 2.0)),
            new Line(new Point(0.8, 1.2), new Point(0.3, 1.2)),
        }),
        
        /* B */ // 0x42
        new List<Line>(new[] {
            new Line(new Point(0.0, 0.0), new Point(0.0, 2.0)),
            new Line(new Point(0.0, 2.0), new Point(1.0, 1.5)),
            new Line(new Point(1.0, 1.5), new Point(1.0, 0.5)),
            new Line(new Point(1.0, 0.5), new Point(0.0, 0.0)),
            new Line(new Point(0.0, 1.0), new Point(1.0, 1.0)),
        }),
        
        /* C */ // 0x43
        new List<Line>(new[] {
            new Line(new Point(1.0, 0.0), new Point(0.0, 0.0)),
            new Line(new Point(0.0, 0.0), new Point(0.0, 2.0)),
            new Line(new Point(0.0, 2.0), new Point(1.0, 2.0)),
        }),
        
        /* D */ // 0x44
        new List<Line>(new[] {
            new Line(new Point(0.0, 0.0), new Point(0.0, 2.0)),
            new Line(new Point(0.0, 2.0), new Point(1.0, 1.5)),
            new Line(new Point(1.0, 1.5), new Point(1.0, 0.5)),
            new Line(new Point(1.0, 0.5), new Point(0.0, 0.0)),
        }),
        
        /* E */ // 0x45
        new List<Line>(new[] {
            new Line(new Point(0.0, 0.0), new Point(0.0, 2.0)),
            new Line(new Point(0.0, 2.0), new Point(1.0, 2.0)),
            new Line(new Point(1.0, 1.0), new Point(0.0, 1.0)),
            new Line(new Point(0.0, 1.0), new Point(0.0, 0.0)),
            new Line(new Point(0.0, 0.0), new Point(1.0, 0.0)),
        }),
        
        /* F */ // 0x46
        new List<Line>(new[] {
            new Line(new Point(0.0, 0.0), new Point(0.0, 2.0)),
            new Line(new Point(1.0, 1.0), new Point(0.0, 1.0)),
            new Line(new Point(0.0, 1.0), new Point(0.0, 0.0)),
            new Line(new Point(0.0, 0.0), new Point(1.0, 0.0)),
        }),
        
        /* G */ // 0x47
        new List<Line>(new[] {
            new Line(new Point(1.0, 0.0), new Point(0.0, 0.0)),
            new Line(new Point(0.0, 0.0), new Point(0.0, 2.0)),
            new Line(new Point(0.0, 2.0), new Point(1.0, 2.0)),
            new Line(new Point(1.0, 2.0), new Point(1.0, 1.0)),
            new Line(new Point(1.0, 1.0), new Point(0.5, 1.0)),
        }),
        
        /* H */ // 0x48
        new List<Line>(new[] {
            new Line(new Point(0.0, 0.0), new Point(0.0, 2.0)),
            new Line(new Point(0.0, 1.0), new Point(1.0, 1.0)),
            new Line(new Point(1.0, 0.0), new Point(1.0, 2.0)),
        }),
        
        /* I */ // 0x49
        new List<Line>(new[] {
            new Line(new Point(0.0, 0.0), new Point(1.0, 0.0)),
            new Line(new Point(0.5, 0.0), new Point(0.5, 2.0)),
            new Line(new Point(0.0, 2.0), new Point(1.0, 2.0)),
        }),
        
        /* J */ // 0x4A
        new List<Line>(new[] {
            new Line(new Point(1.0, 0.0), new Point(1.0, 2.0)),
            new Line(new Point(1.0, 2.0), new Point(0.0, 2.0)),
            new Line(new Point(0.0, 2.0), new Point(0.0, 1.0)),
        }),
        
        /* K */ // 0x4B
        new List<Line>(new[] {
            new Line(new Point(0.0, 0.0), new Point(0.0, 2.0)),
            new Line(new Point(0.0, 1.0), new Point(1.0, 0.0)),
            new Line(new Point(0.0, 1.0), new Point(1.0, 2.0)),
        }),
        
        /* L */ // 0x4C
        new List<Line>(new[] {
            new Line(new Point(0.0, 0.0), new Point(0.0, 2.0)),
            new Line(new Point(0.0, 2.0), new Point(1.0, 2.0)),
        }),
        
        /* M */ // 0x4D
        new List<Line>(new[] {
            new Line(new Point(0.0, 0.0), new Point(0.0, 2.0)),
            new Line(new Point(0.0, 0.0), new Point(0.5, 1.0)),
            new Line(new Point(0.5, 1.0), new Point(1.0, 0.0)),
            new Line(new Point(1.0, 2.0), new Point(1.0, 0.0)),
        }),
        
        /* N */ // 0x4E
        new List<Line>(new[] {
            new Line(new Point(0.0, 0.0), new Point(0.0, 2.0)),
            new Line(new Point(0.0, 0.0), new Point(1.0, 2.0)),
            new Line(new Point(1.0, 0.0), new Point(1.0, 2.0)),
        }),
        
        /* O */ // 0x4F
        new List<Line>(new[] {
            new Line(new Point(0.0, 0.0), new Point(0.0, 2.0)),
            new Line(new Point(0.0, 2.0), new Point(1.0, 2.0)),
            new Line(new Point(1.0, 2.0), new Point(1.0, 0.0)),
            new Line(new Point(1.0, 0.0), new Point(0.0, 0.0)),
        }),
        
        /* P */ // 0x50
        new List<Line>(new[] {
            new Line(new Point(0.0, 0.0), new Point(0.0, 2.0)),
            new Line(new Point(0.0, 0.0), new Point(1.0, 0.0)),
            new Line(new Point(1.0, 0.0), new Point(1.0, 1.0)),
            new Line(new Point(1.0, 1.0), new Point(0.0, 1.0)),
        }),
        
        /* Q */ // 0x51
        new List<Line>(new[] {
            new Line(new Point(0.0, 0.0), new Point(0.0, 2.0)),
            new Line(new Point(0.0, 2.0), new Point(0.5, 2.0)),
            new Line(new Point(1.0, 1.5), new Point(1.0, 0.0)),
            new Line(new Point(1.0, 0.0), new Point(0.0, 0.0)),
            new Line(new Point(0.5, 2.0), new Point(1.0, 1.5)),
            new Line(new Point(0.5, 1.5), new Point(1.0, 2.0)),
        }),
        
        /* R */ // 0x52
        new List<Line>(new[] {
            new Line(new Point(0.0, 2.0), new Point(0.0, 0.0)),
            new Line(new Point(0.0, 0.0), new Point(1.0, 0.0)),
            new Line(new Point(1.0, 0.0), new Point(1.0, 1.0)),
            new Line(new Point(1.0, 1.0), new Point(0.0, 1.0)),
            new Line(new Point(0.0, 1.0), new Point(1.0, 2.0)),
        }),
        
        /* S */ // 0x53
        new List<Line>(new[] {
            new Line(new Point(0.0, 2.0), new Point(1.0, 2.0)),
            new Line(new Point(1.0, 2.0), new Point(1.0, 1.0)),
            new Line(new Point(1.0, 1.0), new Point(0.0, 1.0)),
            new Line(new Point(0.0, 1.0), new Point(0.0, 0.0)),
            new Line(new Point(0.0, 0.0), new Point(1.0, 0.0)),
        }),
        
        /* T */ // 0x54
        new List<Line>(new[] {
            new Line(new Point(0.0, 0.0), new Point(1.0, 0.0)),
            new Line(new Point(0.5, 0.0), new Point(0.5, 2.0)),
        }),
        
        /* U */ // 0x55
        new List<Line>(new[] {
            new Line(new Point(0.0, 0.0), new Point(0.0, 2.0)),
            new Line(new Point(0.0, 2.0), new Point(1.0, 2.0)),
            new Line(new Point(1.0, 2.0), new Point(1.0, 0.0)),
        }),
        
        /* V */ // 0x56
        new List<Line>(new[] {
            new Line(new Point(0.0, 0.0), new Point(0.5, 2.0)),
            new Line(new Point(0.5, 2.0), new Point(1.0, 0.0)),
        }),
        
        /* W */ // 0x57
        new List<Line>(new[] {
            new Line(new Point(0.0, 0.0), new Point(0.0, 2.0)),
            new Line(new Point(0.0, 2.0), new Point(0.5, 1.0)),
            new Line(new Point(0.5, 1.0), new Point(1.0, 2.0)),
            new Line(new Point(1.0, 2.0), new Point(1.0, 0.0)),
        }),
        
        /* X */ // 0x58
        new List<Line>(new[] {
            new Line(new Point(0.0, 0.0), new Point(1.0, 2.0)),
            new Line(new Point(0.0, 2.0), new Point(1.0, 0.0)),
        }),
        
        /* Y */ // 0x59
        new List<Line>(new[] {
            new Line(new Point(0.0, 0.0), new Point(0.5, 1.0)),
            new Line(new Point(0.5, 1.0), new Point(1.0, 0.0)),
            new Line(new Point(0.5, 1.0), new Point(0.5, 2.0)),
        }),
        
        /* Z */ // 0x5A
        new List<Line>(new[] {
            new Line(new Point(0.0, 0.0), new Point(1.0, 0.0)),
            new Line(new Point(1.0, 0.0), new Point(0.0, 2.0)),
            new Line(new Point(0.0, 2.0), new Point(1.0, 2.0)),
        }),
        
        /* [ */ // 0x5B
        new List<Line>(new[] {
            new Line(new Point(0.2, 0.0), new Point(0.2, 2.0)),
            new Line(new Point(0.2, 0.0), new Point(0.8, 0.0)),
            new Line(new Point(0.2, 2.0), new Point(0.8, 2.0)),
        }),
        
        /* \ */ // 0x5C
        new List<Line>(new[] {
            new Line(new Point(0.0, 0.0), new Point(1.0, 2.0)),
        }),
        
        /* ] */ // 0x5D
        new List<Line>(new[] {
            new Line(new Point(0.2, 0.0), new Point(0.8, 0.0)),
            new Line(new Point(0.8, 0.0), new Point(0.8, 2.0)),
            new Line(new Point(0.2, 2.0), new Point(0.8, 2.0)),
        }),
        
        /* ^ */ // 0x5E
        new List<Line>(new[] {
            new Line(new Point(0.5, 0.0), new Point(0.0, 0.8)),
            new Line(new Point(0.5, 0.0), new Point(1.0, 0.8)),
        }),
        
        /* _ */ // 0x5F
        new List<Line>(new[] {
            new Line(new Point(0.0, 2.0), new Point(1.0, 2.0)),
        }),
        
        /* ` */ // 0x60
        new List<Line>(new[] {
            new Line(new Point(0.0, 0.5), new Point(0.5, 0.0)),
        }),
        
        /* a */ // 0x61
        new List<Line>(new[] {
            new Line(new Point(0.0, 1.0), new Point(0.0, 0.0)),
            new Line(new Point(0.0, 0.0), new Point(1.0, 0.0)),
            new Line(new Point(1.0, 0.0), new Point(1.0, 1.0)),
            new Line(new Point(1.0, 1.0), new Point(0.0, 1.0)),
            new Line(new Point(0.0, 1.0), new Point(0.0, 2.0)),
            new Line(new Point(0.0, 2.0), new Point(1.0, 2.0)),
        }),
        
        /* b */ // 0x62
        new List<Line>(new[] {
            new Line(new Point(0.0, 0.0), new Point(0.0, 2.0)),
            new Line(new Point(0.0, 1.0), new Point(1.0, 1.0)),
            new Line(new Point(1.0, 1.0), new Point(1.0, 0.0)),
            new Line(new Point(1.0, 0.0), new Point(0.0, 0.0)),
        }),
        
        /* c */ // 0x63
        new List<Line>(new[] {
            new Line(new Point(1.0, 0.0), new Point(0.0, 0.0)),
            new Line(new Point(0.0, 0.0), new Point(0.0, 2.0)),
            new Line(new Point(0.0, 2.0), new Point(1.0, 2.0)),
        }),
        
        /* d */ // 0x64
        new List<Line>(new[] {
            new Line(new Point(1.0, 0.0), new Point(1.0, 2.0)),
            new Line(new Point(1.0, 1.0), new Point(0.0, 1.0)),
            new Line(new Point(0.0, 1.0), new Point(0.0, 0.0)),
            new Line(new Point(0.0, 0.0), new Point(1.0, 0.0)),
        }),
        
        /* e */ // 0x65
        new List<Line>(new[] {
            new Line(new Point(0.0, 0.0), new Point(1.0, 0.0)),
            new Line(new Point(1.0, 0.0), new Point(1.0, 2.0)),
            new Line(new Point(1.0, 2.0), new Point(0.0, 2.0)),
            new Line(new Point(0.0, 2.0), new Point(0.0, 0.0)),
            new Line(new Point(0.0, 1.0), new Point(1.0, 1.0)),
        }),
        
        /* f */ // 0x66
        new List<Line>(new[] {
            new Line(new Point(0.0, 0.0), new Point(0.0, 2.0)),
            new Line(new Point(0.0, 1.0), new Point(1.0, 1.0)),
            new Line(new Point(0.5, 1.0), new Point(0.5, 2.0)),
        }),
        
        /* g */ // 0x67
        new List<Line>(new[] {
            new Line(new Point(1.0, 0.0), new Point(1.0, 2.0)),
            new Line(new Point(1.0, 2.0), new Point(0.0, 2.0)),
            new Line(new Point(0.0, 2.0), new Point(0.0, 1.0)),
            new Line(new Point(0.0, 1.0), new Point(1.0, 1.0)),
        }),
        
        /* h */ // 0x68
        new List<Line>(new[] {
            new Line(new Point(0.0, 0.0), new Point(0.0, 2.0)),
            new Line(new Point(0.0, 1.0), new Point(1.0, 1.0)),
        }),
        
        /* i */ // 0x69
        new List<Line>(new[] {
            new Line(new Point(0.0, 0.0), new Point(0.0, 1.0)),
            new Line(new Point(0.0, 2.0), new Point(0.0, 1.0)),
        }),
        
        /* j */ // 0x6A
        new List<Line>(new[] {
            new Line(new Point(1.0, 0.0), new Point(1.0, 1.0)),
            new Line(new Point(1.0, 2.0), new Point(1.0, 1.0)),
            new Line(new Point(1.0, 2.0), new Point(0.0, 2.0)),
        }),
        
        /* k */ // 0x6B
        new List<Line>(new[] {
            new Line(new Point(0.0, 0.0), new Point(0.0, 2.0)),
            new Line(new Point(0.0, 1.0), new Point(1.0, 0.0)),
            new Line(new Point(0.0, 1.0), new Point(1.0, 2.0)),
        }),
        
        /* l */ // 0x6C
        new List<Line>(new[] {
            new Line(new Point(0.0, 0.0), new Point(0.0, 2.0)),
        }),
        
        /* m */ // 0x6D
        new List<Line>(new[] {
            new Line(new Point(0.0, 0.0), new Point(0.0, 1.0)),
            new Line(new Point(0.0, 1.0), new Point(1.0, 0.0)),
            new Line(new Point(1.0, 0.0), new Point(1.0, 1.0)),
            new Line(new Point(1.0, 1.0), new Point(0.0, 1.0)),
        }),
        
        /* n */ // 0x6E
        new List<Line>(new[] {
            new Line(new Point(0.0, 0.0), new Point(0.0, 1.0)),
            new Line(new Point(0.0, 1.0), new Point(1.0, 0.0)),
        }),
        
        /* o */ // 0x6F
        new List<Line>(new[] {
            new Line(new Point(0.0, 0.0), new Point(0.0, 2.0)),
            new Line(new Point(0.0, 2.0), new Point(1.0, 2.0)),
            new Line(new Point(1.0, 2.0), new Point(1.0, 0.0)),
            new Line(new Point(1.0, 0.0), new Point(0.0, 0.0)),
        }),
    });
}
