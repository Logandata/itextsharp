using System;
using System.Collections.Generic;
using System.Text;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.error_messages;
/*
 * This file is part of the iText (R) project.
 * Copyright (c) 1998-2011 1T3XT BVBA
 * Authors: Bruno Lowagie, Paulo Soares, et al.
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License version 3
 * as published by the Free Software Foundation with the addition of the
 * following permission added to Section 15 as permitted in Section 7(a):
 * FOR ANY PART OF THE COVERED WORK IN WHICH THE COPYRIGHT IS OWNED BY 1T3XT,
 * 1T3XT DISCLAIMS THE WARRANTY OF NON INFRINGEMENT OF THIRD PARTY RIGHTS.
 *
 * This program is distributed in the hope that it will be useful, but
 * WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
 * or FITNESS FOR A PARTICULAR PURPOSE.
 * See the GNU Affero General Public License for more details.
 * You should have received a copy of the GNU Affero General Public License
 * along with this program; if not, see http://www.gnu.org/licenses or write to
 * the Free Software Foundation, Inc., 51 Franklin Street, Fifth Floor,
 * Boston, MA, 02110-1301 USA, or download the license from the following URL:
 * http://itextpdf.com/terms-of-use/
 *
 * The interactive user interfaces in modified source and object code versions
 * of this program must display Appropriate Legal Notices, as required under
 * Section 5 of the GNU Affero General Public License.
 *
 * In accordance with Section 7(b) of the GNU Affero General Public License,
 * a covered work must retain the producer line in every PDF that is created
 * or manipulated using iText.
 *
 * You can be released from the requirements of the license by purchasing
 * a commercial license. Buying such a license is mandatory as soon as you
 * develop commercial activities involving the iText software without
 * disclosing the source code of your own applications.
 * These activities include: offering paid services to customers as an ASP,
 * serving PDFs on the fly in a web application, shipping iText with a closed
 * source product.
 *
 * For more information, please contact iText Software Corp. at this
 * address: sales@itextpdf.com
 */
namespace iTextSharp.text.pdf.fonts.cmaps {

    public class CMapByteCid : AbstractCMap {
        private List<char[]> planes = new List<char[]>();

        public CMapByteCid() {
            planes.Add(new char[256]);
        }
        
        internal override void AddChar(PdfString mark, PdfObject code) {
            if (!(code is PdfNumber))
                return;
            EncodeSequence(DecodeStringToByte(mark), (char)((PdfNumber)code).IntValue);
        }
        
        private void EncodeSequence(byte[] seqs, char cid) {
            int size = seqs.Length - 1;
            int nextPlane = 0;
            int one;
            char[] plane;
            for (int idx = 0; idx < size; ++idx) {
                plane = planes[nextPlane];
                one = seqs[idx] & 0xff;
                char c = plane[one];
                if (c != 0 && (c & 0x8000) == 0)
                    throw new ArgumentException(MessageLocalization.GetComposedMessage("inconsistent.mapping"));
                if (c == 0) {
                    planes.Add(new char[256]);
                    c = (char)(planes.Count - 1 | 0x8000);
                    plane[one] = c;
                }
                nextPlane = c & 0x7fff;
            }
            plane = planes[nextPlane];
            one = seqs[size] & 0xff;
            char c2 = plane[one];
            if ((c2 & 0x8000) != 0)
                throw new ArgumentException(MessageLocalization.GetComposedMessage("inconsistent.mapping"));
            plane[one] = cid;
        }
        
        /**
         * 
         * @param seq
         * @return the cid code or -1 for end
         */
        public int DecodeSingle(CMapSequence seq) {
            int end = seq.off + seq.len;
            int currentPlane = 0;
            while (seq.off < end) {
                int one = seq.seq[seq.off++] & 0xff;
                --seq.len;
                char[] plane = planes[currentPlane];
                int cid = plane[one];
                if ((cid & 0x8000) == 0) {
                    return cid;
                }
                else
                    currentPlane = cid & 0x7fff;
            }
            return -1;
        }

        public String DecodeSequence(CMapSequence seq) {
            StringBuilder sb = new StringBuilder();
            int cid = 0;
            while ((cid = DecodeSingle(seq)) >= 0) {
                sb.Append((char)cid);
            }
            return sb.ToString();
        }
    }
}