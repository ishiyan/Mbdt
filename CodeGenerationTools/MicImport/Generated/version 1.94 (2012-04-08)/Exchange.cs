using System;
using System.Runtime.Serialization;

namespace Mbst.Core
{
    /// <summary>
    /// ISO 10383 Market Identifier Code (MIC) utilities.
    /// <para>Generated automatically from the list of ISO 10383 Exchange/Market Identifier Codes, version 1.94 (2012-04-08).</para>
    /// <para><c>http://www.iso15022.org/MIC/homepageMIC.htm</c></para>
    /// </summary>
    [DataContract]
    public sealed class Exchange : IComparable<Exchange>, IEquatable<Exchange>
    {
        #region Members and accessors
        #region Mic
        [DataMember]
        private readonly ExchangeMic mic = ExchangeMic.Xams;
        /// <summary>
        /// ISO 10383 Market Identifier Code (MIC).
        /// </summary>
        public ExchangeMic Mic { get { return mic; } }
        #endregion

        #region TimeZone
        /// <summary>
        /// The time zone.
        /// </summary>
        public TimeSpan TimeZone
        {
            get
            {
                switch (mic)
                {
                    // AL, Albania
                    case ExchangeMic.Xtir:
                    // DZ, Algeria
                    case ExchangeMic.Xalg:
                    // AT, Austria
                    case ExchangeMic.Exaa:
                    case ExchangeMic.Wbah:
                    case ExchangeMic.Wbdm:
                    case ExchangeMic.Wbgf:
                    case ExchangeMic.Xceg:
                    case ExchangeMic.Xvie:
                    case ExchangeMic.Xwbo:
                    // BE, Belgium
                    case ExchangeMic.Alxb:
                    case ExchangeMic.Blpx:
                    case ExchangeMic.Bmts:
                    case ExchangeMic.Enxb:
                    case ExchangeMic.Frrf:
                    case ExchangeMic.Mlxb:
                    case ExchangeMic.Mtsd:
                    case ExchangeMic.Mtsf:
                    case ExchangeMic.Tnlb:
                    case ExchangeMic.Vpxb:
                    case ExchangeMic.Xbrd:
                    case ExchangeMic.Xbru:
                    // CZ, Czech Republic
                    case ExchangeMic.Spad:
                    case ExchangeMic.Xpra:
                    case ExchangeMic.Xpxe:
                    case ExchangeMic.Xrmo:
                    case ExchangeMic.Xrmz:
                    // DK, Denmark
                    case ExchangeMic.Damp:
                    case ExchangeMic.Dktc:
                    case ExchangeMic.Gxgm:
                    case ExchangeMic.Npga:
                    case ExchangeMic.Xcse:
                    case ExchangeMic.Xfnd:
                    // FR, France
                    case ExchangeMic.Alxp:
                    case ExchangeMic.Coal:
                    case ExchangeMic.Epex:
                    case ExchangeMic.Fmts:
                    case ExchangeMic.Gmtf:
                    case ExchangeMic.Mtch:
                    case ExchangeMic.Xafr:
                    case ExchangeMic.Xbln:
                    case ExchangeMic.Xmat:
                    case ExchangeMic.Xmli:
                    case ExchangeMic.Xmon:
                    case ExchangeMic.Xpar:
                    case ExchangeMic.Xpow:
                    // DE, Germany
                    case ExchangeMic.X360T:
                    case ExchangeMic.Bera:
                    case ExchangeMic.Berb:
                    case ExchangeMic.Berc:
                    case ExchangeMic.Cats:
                    case ExchangeMic.Dbox:
                    case ExchangeMic.Dusa:
                    case ExchangeMic.Dusb:
                    case ExchangeMic.Dusc:
                    case ExchangeMic.Dusd:
                    case ExchangeMic.Ecag:
                    case ExchangeMic.Eqta:
                    case ExchangeMic.Eqtb:
                    case ExchangeMic.Eqtc:
                    case ExchangeMic.Eqtd:
                    case ExchangeMic.Euwx:
                    case ExchangeMic.Fraa:
                    case ExchangeMic.Frab:
                    case ExchangeMic.Frad:
                    case ExchangeMic.Gmex:
                    case ExchangeMic.Hama:
                    case ExchangeMic.Hamb:
                    case ExchangeMic.Hana:
                    case ExchangeMic.Hanb:
                    case ExchangeMic.Muna:
                    case ExchangeMic.Munb:
                    case ExchangeMic.Plus:
                    case ExchangeMic.Stua:
                    case ExchangeMic.Stub:
                    case ExchangeMic.Xber:
                    case ExchangeMic.Xdbc:
                    case ExchangeMic.Xdbv:
                    case ExchangeMic.Xdbx:
                    case ExchangeMic.Xdus:
                    case ExchangeMic.Xeee:
                    case ExchangeMic.Xeqt:
                    case ExchangeMic.Xeta:
                    case ExchangeMic.Xetb:
                    case ExchangeMic.Xetc:
                    case ExchangeMic.Xetd:
                    case ExchangeMic.Xeti:
                    case ExchangeMic.Xetr:
                    case ExchangeMic.Xeub:
                    case ExchangeMic.Xeum:
                    case ExchangeMic.Xeup:
                    case ExchangeMic.Xeur:
                    case ExchangeMic.Xfra:
                    case ExchangeMic.Xgat:
                    case ExchangeMic.Xgrm:
                    case ExchangeMic.Xham:
                    case ExchangeMic.Xhan:
                    case ExchangeMic.Xinv:
                    case ExchangeMic.Xmun:
                    case ExchangeMic.Xnew:
                    case ExchangeMic.Xsc1:
                    case ExchangeMic.Xsc2:
                    case ExchangeMic.Xsc3:
                    case ExchangeMic.Xstu:
                    case ExchangeMic.Xxsc:
                    case ExchangeMic.Zobx:
                    // IT, Italy
                    case ExchangeMic.Bond:
                    case ExchangeMic.Emdr:
                    case ExchangeMic.Emid:
                    case ExchangeMic.Emir:
                    case ExchangeMic.Etfp:
                    case ExchangeMic.Etlx:
                    case ExchangeMic.Hmod:
                    case ExchangeMic.Hmtf:
                    case ExchangeMic.Macx:
                    case ExchangeMic.Mivx:
                    case ExchangeMic.Motx:
                    case ExchangeMic.Mtaa:
                    case ExchangeMic.Mtsc:
                    case ExchangeMic.Mtsm:
                    case ExchangeMic.Sedx:
                    case ExchangeMic.Ssob:
                    case ExchangeMic.Xaim:
                    case ExchangeMic.Xdmi:
                    case ExchangeMic.Xgme:
                    case ExchangeMic.Xmot:
                    // SK, Slovakia
                    case ExchangeMic.Xbra:
                    // ES, Spain
                    case ExchangeMic.Mabx:
                    case ExchangeMic.Omel:
                    case ExchangeMic.Pave:
                    case ExchangeMic.Send:
                    case ExchangeMic.Xbar:
                    case ExchangeMic.Xbil:
                    case ExchangeMic.Xdpa:
                    case ExchangeMic.Xdrf:
                    case ExchangeMic.Xlat:
                    case ExchangeMic.Xmad:
                    case ExchangeMic.Xmce:
                    case ExchangeMic.Xmef:
                    case ExchangeMic.Xmrv:
                    case ExchangeMic.Xnaf:
                    case ExchangeMic.Xsrm:
                    case ExchangeMic.Xval:
                    // SE, Sweden
                    case ExchangeMic.Burg:
                    case ExchangeMic.Burm:
                    case ExchangeMic.Fnse:
                    case ExchangeMic.Nmtf:
                    case ExchangeMic.Xndx:
                    case ExchangeMic.Xngm:
                    case ExchangeMic.Xnmr:
                    case ExchangeMic.Xopv:
                    case ExchangeMic.Xsat:
                    case ExchangeMic.Xsto:
                    // CH, Switzerland
                    case ExchangeMic.Xbrn:
                    case ExchangeMic.Xqmh:
                    case ExchangeMic.Xscu:
                    case ExchangeMic.Xstv:
                    case ExchangeMic.Xstx:
                    case ExchangeMic.Xswx:
                    case ExchangeMic.Xvtx:
                    case ExchangeMic.Zkbx:
                    // NL, The Netherlands
                    case ExchangeMic.Alxa:
                    case ExchangeMic.Clmx:
                    case ExchangeMic.Ecxe:
                    case ExchangeMic.Ndex:
                    case ExchangeMic.Nlpx:
                    case ExchangeMic.Tnla:
                    case ExchangeMic.Tomd:
                    case ExchangeMic.Tomx:
                    case ExchangeMic.Xams:
                    case ExchangeMic.Xeuc:
                    case ExchangeMic.Xeue:
                    case ExchangeMic.Xeui:
                    case ExchangeMic.Xhft:
                    // RS, Serbia
                    case ExchangeMic.Xbel:
                    // NG, Nigeria
                    case ExchangeMic.Xnsa:
                    // PL, Poland
                    case ExchangeMic.Bosp:
                    case ExchangeMic.Mtsp:
                    case ExchangeMic.Plpx:
                    case ExchangeMic.Poee:
                    case ExchangeMic.Rpwc:
                    case ExchangeMic.Tbsp:
                    case ExchangeMic.Xnco:
                    case ExchangeMic.Xwar:
                    // NO, Norway
                    case ExchangeMic.Fish:
                    case ExchangeMic.Fshx:
                    case ExchangeMic.Icas:
                    case ExchangeMic.Nops:
                    case ExchangeMic.Norx:
                    case ExchangeMic.Notc:
                    case ExchangeMic.Xima:
                    case ExchangeMic.Xoam:
                    case ExchangeMic.Xoas:
                    case ExchangeMic.Xosl:
                    // NA, Namibia
                    case ExchangeMic.Xnam:
                    // ME, Montenegro
                    case ExchangeMic.Xmnx:
                    // MT, Malta
                    case ExchangeMic.Ewsm:
                    case ExchangeMic.Xmal:
                    // MK, Macedonia
                    case ExchangeMic.Xmae:
                    // LU, Luxembourg
                    case ExchangeMic.Cclx:
                    case ExchangeMic.Emtf:
                    case ExchangeMic.Xlux:
                    case ExchangeMic.Xves:
                    // HU, Hungary
                    case ExchangeMic.Beta:
                    case ExchangeMic.Hupx:
                    case ExchangeMic.Qmtf:
                    case ExchangeMic.Xbud:
                    // BA, Bosnia and Herzegovina
                    case ExchangeMic.Xblb:
                    case ExchangeMic.Xsse:
                    // HR, Croatia
                    case ExchangeMic.Xtrz:
                    case ExchangeMic.Xzag:
                    case ExchangeMic.Xzam:
                    // TN, Tunisia
                    case ExchangeMic.Xtun:
                    // CM, Cameroon
                    case ExchangeMic.Xdsx:
                        return new TimeSpan(1, 0, 0);
                    // BY, Belarus
                    case ExchangeMic.Bcse:
                    // CY, Cyprus
                    case ExchangeMic.Xcyo:
                    case ExchangeMic.Xcys:
                    case ExchangeMic.Xecm:
                    // EE, Estonia
                    case ExchangeMic.Fnee:
                    case ExchangeMic.Xtal:
                    // GR, Greece
                    case ExchangeMic.Enax:
                    case ExchangeMic.Euax:
                    case ExchangeMic.Hdat:
                    case ExchangeMic.Hotc:
                    case ExchangeMic.Xade:
                    case ExchangeMic.Xath:
                    // FI, Finland
                    case ExchangeMic.Fnfi:
                    case ExchangeMic.Xhel:
                    // IL, Israel
                    case ExchangeMic.Xtae:
                    // LB, Lebanon
                    case ExchangeMic.Xbey:
                    // LV, Latvia
                    case ExchangeMic.Fnlv:
                    case ExchangeMic.Xris:
                    // LT, Lithuania
                    case ExchangeMic.Bapx:
                    case ExchangeMic.Fnlt:
                    case ExchangeMic.Nasb:
                    case ExchangeMic.Xlit:
                    // MD, Moldova
                    case ExchangeMic.Xmol:
                    // ZA, South Africa
                    case ExchangeMic.Altx:
                    case ExchangeMic.Xbes:
                    case ExchangeMic.Xjse:
                    case ExchangeMic.Xsaf:
                    case ExchangeMic.Xsfa:
                    case ExchangeMic.Yldx:
                    // TR, Turkey
                    case ExchangeMic.Xiab:
                    case ExchangeMic.Xist:
                    case ExchangeMic.Xtur:
                    // UA, Ukraine
                    case ExchangeMic.Eese:
                    case ExchangeMic.Pftq:
                    case ExchangeMic.Pfts:
                    case ExchangeMic.Sepe:
                    case ExchangeMic.Ukex:
                    case ExchangeMic.Xdfb:
                    case ExchangeMic.Xkhr:
                    case ExchangeMic.Xkie:
                    case ExchangeMic.Xkis:
                    case ExchangeMic.Xode:
                    case ExchangeMic.Xpri:
                    case ExchangeMic.Xuax:
                    case ExchangeMic.Xukr:
                    // RO, Romania
                    case ExchangeMic.Bmfa:
                    case ExchangeMic.Bmfm:
                    case ExchangeMic.Bmfx:
                    case ExchangeMic.Sbmf:
                    case ExchangeMic.Xbrm:
                    case ExchangeMic.Xbsd:
                    case ExchangeMic.Xbse:
                    case ExchangeMic.Xcan:
                    case ExchangeMic.Xras:
                    case ExchangeMic.Xrpm:
                    // RW, Rwanda
                    case ExchangeMic.Rotc:
                    case ExchangeMic.Rsex:
                    // PS, Palestinian Territory
                    case ExchangeMic.Xpae:
                    // JO, Jordan
                    case ExchangeMic.Xamm:
                    // EG, Egypt
                    case ExchangeMic.Nilx:
                    case ExchangeMic.Xcai:
                    // BG, Bulgaria
                    case ExchangeMic.Xbul:
                    // SZ, Swaziland
                    case ExchangeMic.Xswa:
                    // MZ, Mozambique
                    case ExchangeMic.Xmap:
                        return new TimeSpan(2, 0, 0);
                    // RU, Russia
                    case ExchangeMic.Ixsp:
                    case ExchangeMic.Misx:
                    case ExchangeMic.Namx:
                    case ExchangeMic.Nncs:
                    case ExchangeMic.Rpdx:
                    case ExchangeMic.Rtsx:
                    case ExchangeMic.Spim:
                    case ExchangeMic.Xapi:
                    case ExchangeMic.Xmos:
                    case ExchangeMic.Xpet:
                    case ExchangeMic.Xpic:
                    case ExchangeMic.Xrus:
                    case ExchangeMic.Xsam:
                    case ExchangeMic.Xsib:
                    // QA, Qatar
                    case ExchangeMic.Dsmd:
                    // KW, Kuwait
                    case ExchangeMic.Xkuw:
                    // KE, Kenya
                    case ExchangeMic.Xnai:
                    // IQ, Iraq
                    case ExchangeMic.Xiqs:
                    // BH, Bahrain
                    case ExchangeMic.Bfex:
                    case ExchangeMic.Xbah:
                    // UG, Uganda
                    case ExchangeMic.Xuga:
                    // TZ, Tanzania
                    case ExchangeMic.Xdar:
                    // SD, Sudan
                    case ExchangeMic.Xkha:
                    // MG, Madagascar
                    case ExchangeMic.Xmdg:
                    // SY, Syria
                    case ExchangeMic.Xdse:
                        return new TimeSpan(3, 0, 0);
                    // IR, Iran
                    case ExchangeMic.Imex:
                    case ExchangeMic.Xteh:
                        return new TimeSpan(3, 30, 0);
                    // AE, Arab Emirates
                    case ExchangeMic.Dgcx:
                    case ExchangeMic.Difx:
                    case ExchangeMic.Dumx:
                    case ExchangeMic.Xads:
                    case ExchangeMic.Xdfm:
                    // KN, Saint Kitts and Nevis
                    case ExchangeMic.Xecs:
                    // MU, Mauritius
                    case ExchangeMic.Gbot:
                    case ExchangeMic.Xmau:
                    // AM, Armenia
                    case ExchangeMic.Xarm:
                    // AZ, Azerbaijan
                    case ExchangeMic.Bsex:
                    case ExchangeMic.Xibe:
                    // OM, Oman
                    case ExchangeMic.Xmus:
                        return new TimeSpan(4, 0, 0);
                    // PK, Pakistan
                    case ExchangeMic.Ncel:
                    case ExchangeMic.Xisl:
                    case ExchangeMic.Xkar:
                    case ExchangeMic.Xlah:
                    // UZ, Uzbekistan
                    case ExchangeMic.Xcet:
                    case ExchangeMic.Xcue:
                    case ExchangeMic.Xkce:
                    case ExchangeMic.Xste:
                    case ExchangeMic.Xuni:
                        return new TimeSpan(5, 0, 0);
                    // LK, Sri Lanka
                    case ExchangeMic.Xcol:
                    // IN, India
                    case ExchangeMic.Acex:
                    case ExchangeMic.Bsme:
                    case ExchangeMic.Icxl:
                    case ExchangeMic.Isex:
                    case ExchangeMic.Mcxx:
                    case ExchangeMic.Nbot:
                    case ExchangeMic.Nmce:
                    case ExchangeMic.Otcx:
                    case ExchangeMic.Pxil:
                    case ExchangeMic.Xban:
                    case ExchangeMic.Xbom:
                    case ExchangeMic.Xcal:
                    case ExchangeMic.Xdes:
                    case ExchangeMic.Ximc:
                    case ExchangeMic.Xmds:
                    case ExchangeMic.Xncd:
                    case ExchangeMic.Xnse:
                    case ExchangeMic.Xuse:
                        return new TimeSpan(5, 30, 0);
                    // NP, Nepal
                    case ExchangeMic.Xnep:
                        return new TimeSpan(5, 45, 0);
                    // KG, Kyrgyzstan
                    case ExchangeMic.Xkse:
                    // KZ, Kazakhstan
                    case ExchangeMic.Etsc:
                    case ExchangeMic.Xkaz:
                        return new TimeSpan(6, 0, 0);
                    // SG, Singapore
                    case ExchangeMic.Chie:
                    case ExchangeMic.Cltd:
                    case ExchangeMic.Jadx:
                    case ExchangeMic.Smex:
                    case ExchangeMic.Tfsa:
                    case ExchangeMic.Xsca:
                    case ExchangeMic.Xsce:
                    case ExchangeMic.Xscl:
                    case ExchangeMic.Xses:
                    case ExchangeMic.Xsim:
                    // ID, Indonesia
                    case ExchangeMic.Icdx:
                    case ExchangeMic.Xbbj:
                    case ExchangeMic.Xidx:
                    case ExchangeMic.Xjnb:
                    // TH, Thailand
                    case ExchangeMic.Afet:
                    case ExchangeMic.Beex:
                    case ExchangeMic.Tfex:
                    case ExchangeMic.Xbkf:
                    case ExchangeMic.Xbkk:
                    case ExchangeMic.Xmai:
                        return new TimeSpan(7, 0, 0);
                    // PH, Philippines
                    case ExchangeMic.Pdex:
                    case ExchangeMic.Xphs:
                    // MN, Mongolia
                    case ExchangeMic.Xula:
                    // MY, Malaysia
                    case ExchangeMic.Mesq:
                    case ExchangeMic.Xkls:
                    case ExchangeMic.Xlfx:
                    case ExchangeMic.Xrbm:
                    // HK, Hong Kong
                    case ExchangeMic.Cgmh:
                    case ExchangeMic.Dbhk:
                    case ExchangeMic.Eotc:
                    case ExchangeMic.Hkme:
                    case ExchangeMic.Hsxa:
                    case ExchangeMic.Mehk:
                    case ExchangeMic.Tocp:
                    case ExchangeMic.Ubsx:
                    case ExchangeMic.Xcgs:
                    case ExchangeMic.Xgem:
                    case ExchangeMic.Xhkf:
                    case ExchangeMic.Xhkg:
                    case ExchangeMic.Xihk:
                    case ExchangeMic.Xpst:
                    case ExchangeMic.Sigh:
                    // CN, China
                    case ExchangeMic.Ccfx:
                    case ExchangeMic.Cssx:
                    case ExchangeMic.Sgex:
                    case ExchangeMic.Xcfe:
                    case ExchangeMic.Xdce:
                    case ExchangeMic.Xsge:
                    case ExchangeMic.Xshe:
                    case ExchangeMic.Xshg:
                    case ExchangeMic.Xzce:
                    // BD, Bangladesh
                    case ExchangeMic.Xchg:
                    case ExchangeMic.Xdha:
                    // TW, Taiwan
                    case ExchangeMic.Roco:
                    case ExchangeMic.Xtaf:
                    case ExchangeMic.Xtai:
                        return new TimeSpan(8, 0, 0);
                    // KR, Korea
                    case ExchangeMic.Xkfb:
                    case ExchangeMic.Xkfe:
                    case ExchangeMic.Xkos:
                    case ExchangeMic.Xkrx:
                    // JP, Japan
                    case ExchangeMic.Chij:
                    case ExchangeMic.Citd:
                    case ExchangeMic.Citx:
                    case ExchangeMic.Jasr:
                    case ExchangeMic.Kabu:
                    case ExchangeMic.Sbij:
                    case ExchangeMic.Sigj:
                    case ExchangeMic.Vkab:
                    case ExchangeMic.Xfka:
                    case ExchangeMic.Xijp:
                    case ExchangeMic.Xjas:
                    case ExchangeMic.Xkac:
                    case ExchangeMic.Xngo:
                    case ExchangeMic.Xnks:
                    case ExchangeMic.Xose:
                    case ExchangeMic.Xosj:
                    case ExchangeMic.Xsap:
                    case ExchangeMic.Xsbi:
                    case ExchangeMic.Xtam:
                    case ExchangeMic.Xtff:
                    case ExchangeMic.Xtk1:
                    case ExchangeMic.Xtk2:
                    case ExchangeMic.Xtk3:
                    case ExchangeMic.Xtko:
                    case ExchangeMic.Xtks:
                    case ExchangeMic.Xtkt:
                        return new TimeSpan(9, 0, 0);
                    // AU, Australia
                    case ExchangeMic.Apxl:
                    case ExchangeMic.Asxc:
                    case ExchangeMic.Asxp:
                    case ExchangeMic.Asxv:
                    case ExchangeMic.Awbx:
                    case ExchangeMic.Awex:
                    case ExchangeMic.Chia:
                    case ExchangeMic.Maqx:
                    case ExchangeMic.Meau:
                    case ExchangeMic.Nsxb:
                    case ExchangeMic.Siga:
                    case ExchangeMic.Simv:
                    case ExchangeMic.Xasx:
                    case ExchangeMic.Xnec:
                    case ExchangeMic.Xsfe:
                    case ExchangeMic.Xyie:
                        return new TimeSpan(10, 0, 0);
                    // FJ, Fiji
                    case ExchangeMic.Xsps:
                    // NZ, New Zealand
                    case ExchangeMic.Nzfx:
                    case ExchangeMic.Xnze:
                        return new TimeSpan(12, 0, 0);
                    // CV, Cape Verde
                    case ExchangeMic.Xbvc:
                        return new TimeSpan(-1, 0, 0);
                    // PY, Paraguay
                    case ExchangeMic.Xvpa:
                    // BR, Brazil
                    case ExchangeMic.Bcmm:
                    case ExchangeMic.Bovm:
                    case ExchangeMic.Brix:
                    case ExchangeMic.Bvmf:
                    case ExchangeMic.Ceti:
                    case ExchangeMic.Selc:
                    // UY, Uruguay
                    case ExchangeMic.Bvur:
                    case ExchangeMic.Xmnt:
                    // AR, Argentina
                    case ExchangeMic.Bace:
                    case ExchangeMic.Bcfs:
                    case ExchangeMic.Mvcx:
                    case ExchangeMic.Rofx:
                    case ExchangeMic.Xbcc:
                    case ExchangeMic.Xbcm:
                    case ExchangeMic.Xbcx:
                    case ExchangeMic.Xbue:
                    case ExchangeMic.Xcnf:
                    case ExchangeMic.Xmab:
                    case ExchangeMic.Xmev:
                    case ExchangeMic.Xmtb:
                    case ExchangeMic.Xmvl:
                    case ExchangeMic.Xros:
                    case ExchangeMic.Xrox:
                    case ExchangeMic.Xtuc:
                        return new TimeSpan(-3, 0, 0);
                    // PG, Papua New Guinea
                    case ExchangeMic.Xpom:
                    // CI, Ivory Coast
                    case ExchangeMic.Xbrv:
                    // DO, Dominican Republic
                    case ExchangeMic.Xbvr:
                    // CL, Chile
                    case ExchangeMic.Bova:
                    case ExchangeMic.Xbcl:
                    case ExchangeMic.Xsgo:
                    // BO, Bolivia
                    case ExchangeMic.Xbol:
                    // BM, Bermuda
                    case ExchangeMic.Xbda:
                        return new TimeSpan(-4, 0, 0);
                    // VE, Venezuela
                    case ExchangeMic.Bvca:
                    case ExchangeMic.Xcar:
                    // TT, Trinidad and Tobago
                    case ExchangeMic.Xtrn:
                        return new TimeSpan(-4, -30, 0);
                    // BB, Barbados
                    case ExchangeMic.Xbab:
                    // KY, Cayman Islands
                    case ExchangeMic.Xcay:
                    // PA, Panama
                    case ExchangeMic.Xpty:
                    // PE, Peru
                    case ExchangeMic.Xlim:
                    // JM, Jamaica
                    case ExchangeMic.Xjam:
                    // BS, Bahamas
                    case ExchangeMic.Xbaa:
                    // EC, Ecuador
                    case ExchangeMic.Xgua:
                    case ExchangeMic.Xqui:
                    // CO, Colombia
                    case ExchangeMic.Xbog:
                    // CA, Canada
                    case ExchangeMic.Atsa:
                    case ExchangeMic.Canx:
                    case ExchangeMic.Chic:
                    case ExchangeMic.Ifca:
                    case ExchangeMic.Lica:
                    case ExchangeMic.Matn:
                    case ExchangeMic.Ngxc:
                    case ExchangeMic.Omga:
                    case ExchangeMic.Pure:
                    case ExchangeMic.Tmxs:
                    case ExchangeMic.Xats:
                    case ExchangeMic.Xbbk:
                    case ExchangeMic.Xcnq:
                    case ExchangeMic.Xicx:
                    case ExchangeMic.Xmoc:
                    case ExchangeMic.Xmod:
                    case ExchangeMic.Xtnx:
                    case ExchangeMic.Xtse:
                    case ExchangeMic.Xtsx:
                    // US, United States
                    case ExchangeMic.Aats:
                    case ExchangeMic.Aldp:
                    case ExchangeMic.Amxo:
                    case ExchangeMic.Aqua:
                    case ExchangeMic.Arcd:
                    case ExchangeMic.Arco:
                    case ExchangeMic.Arcx:
                    case ExchangeMic.Baml:
                    case ExchangeMic.Bard:
                    case ExchangeMic.Barx:
                    case ExchangeMic.Bato:
                    case ExchangeMic.Bats:
                    case ExchangeMic.Baty:
                    case ExchangeMic.Bgcf:
                    case ExchangeMic.Bids:
                    case ExchangeMic.Bltd:
                    case ExchangeMic.Bndd:
                    case ExchangeMic.Bosd:
                    case ExchangeMic.Btec:
                    case ExchangeMic.C2Ox:
                    case ExchangeMic.Caes:
                    case ExchangeMic.Cbsx:
                    case ExchangeMic.Ccfe:
                    case ExchangeMic.Cded:
                    case ExchangeMic.Cdel:
                    case ExchangeMic.Cgmi:
                    case ExchangeMic.Cgmu:
                    case ExchangeMic.Cicx:
                    case ExchangeMic.Cslp:
                    case ExchangeMic.Dbsx:
                    case ExchangeMic.Deal:
                    case ExchangeMic.Eddp:
                    case ExchangeMic.Edga:
                    case ExchangeMic.Edgd:
                    case ExchangeMic.Edgx:
                    case ExchangeMic.Eris:
                    case ExchangeMic.Fcbt:
                    case ExchangeMic.Fcme:
                    case ExchangeMic.Finn:
                    case ExchangeMic.Fino:
                    case ExchangeMic.Finr:
                    case ExchangeMic.Finy:
                    case ExchangeMic.Fxal:
                    case ExchangeMic.Fxcm:
                    case ExchangeMic.Glbx:
                    case ExchangeMic.Gllc:
                    case ExchangeMic.Govx:
                    case ExchangeMic.Gree:
                    case ExchangeMic.Gtco:
                    case ExchangeMic.Hegx:
                    case ExchangeMic.Hsfx:
                    case ExchangeMic.Iblx:
                    case ExchangeMic.Icbx:
                    case ExchangeMic.Icel:
                    case ExchangeMic.Icro:
                    case ExchangeMic.Iepa:
                    case ExchangeMic.Ifus:
                    case ExchangeMic.Iidx:
                    case ExchangeMic.Imag:
                    case ExchangeMic.Imbd:
                    case ExchangeMic.Imco:
                    case ExchangeMic.Imcr:
                    case ExchangeMic.Imen:
                    case ExchangeMic.Imeq:
                    case ExchangeMic.Imfx:
                    case ExchangeMic.Imir:
                    case ExchangeMic.Itgi:
                    case ExchangeMic.Jpmx:
                    case ExchangeMic.Kncm:
                    case ExchangeMic.Knem:
                    case ExchangeMic.Knli:
                    case ExchangeMic.Knmx:
                    case ExchangeMic.Lafd:
                    case ExchangeMic.Lafl:
                    case ExchangeMic.Lafx:
                    case ExchangeMic.Levl:
                    case ExchangeMic.Mspl:
                    case ExchangeMic.Msrp:
                    case ExchangeMic.Mstc:
                    case ExchangeMic.Nasd:
                    case ExchangeMic.Nfsa:
                    case ExchangeMic.Nfsc:
                    case ExchangeMic.Nfsd:
                    case ExchangeMic.Nodx:
                    case ExchangeMic.Nxus:
                    case ExchangeMic.Nyfx:
                    case ExchangeMic.Nypc:
                    case ExchangeMic.Nysd:
                    case ExchangeMic.Opra:
                    case ExchangeMic.Otcb:
                    case ExchangeMic.Otcq:
                    case ExchangeMic.Pdqd:
                    case ExchangeMic.Pdqx:
                    case ExchangeMic.Pinx:
                    case ExchangeMic.Pipe:
                    case ExchangeMic.Prse:
                    case ExchangeMic.Psgm:
                    case ExchangeMic.Pulx:
                    case ExchangeMic.Ricd:
                    case ExchangeMic.Ricx:
                    case ExchangeMic.Sgma:
                    case ExchangeMic.Shad:
                    case ExchangeMic.Shaw:
                    case ExchangeMic.Sigx:
                    case ExchangeMic.Sstx:
                    case ExchangeMic.Tfsu:
                    case ExchangeMic.Trck:
                    case ExchangeMic.Trfx:
                    case ExchangeMic.Trwb:
                    case ExchangeMic.Ubsa:
                    case ExchangeMic.Ubsp:
                    case ExchangeMic.Vtex:
                    case ExchangeMic.Xadf:
                    case ExchangeMic.Xaqs:
                    case ExchangeMic.Xase:
                    case ExchangeMic.Xbos:
                    case ExchangeMic.Xbox:
                    case ExchangeMic.Xbrt:
                    case ExchangeMic.Xbxo:
                    case ExchangeMic.Xcbf:
                    case ExchangeMic.Xcbo:
                    case ExchangeMic.Xcbt:
                    case ExchangeMic.Xccx:
                    case ExchangeMic.Xcec:
                    case ExchangeMic.Xcff:
                    case ExchangeMic.Xchi:
                    case ExchangeMic.Xcis:
                    case ExchangeMic.Xcme:
                    case ExchangeMic.Xcur:
                    case ExchangeMic.Xelx:
                    case ExchangeMic.Xfci:
                    case ExchangeMic.Ximm:
                    case ExchangeMic.Xiom:
                    case ExchangeMic.Xisa:
                    case ExchangeMic.Xise:
                    case ExchangeMic.Xisx:
                    case ExchangeMic.Xkbt:
                    case ExchangeMic.Xmer:
                    case ExchangeMic.Xmge:
                    case ExchangeMic.Xmio:
                    case ExchangeMic.Xnas:
                    case ExchangeMic.Xncm:
                    case ExchangeMic.Xndq:
                    case ExchangeMic.Xngs:
                    case ExchangeMic.Xnim:
                    case ExchangeMic.Xnli:
                    case ExchangeMic.Xnms:
                    case ExchangeMic.Xnye:
                    case ExchangeMic.Xnyl:
                    case ExchangeMic.Xnym:
                    case ExchangeMic.Xnys:
                    case ExchangeMic.Xoch:
                    case ExchangeMic.Xotc:
                    case ExchangeMic.Xpbt:
                    case ExchangeMic.Xphl:
                    case ExchangeMic.Xpho:
                    case ExchangeMic.Xpor:
                    case ExchangeMic.Xsef:
                    case ExchangeMic.Xwee:
                        return new TimeSpan(-5, 0, 0);
                    // HN, Honduras
                    case ExchangeMic.Xbcv:
                    // MX, Mexico
                    case ExchangeMic.Cgmx:
                    case ExchangeMic.Xemd:
                    case ExchangeMic.Xmex:
                    // NI, Nicaragua
                    case ExchangeMic.Xman:
                    // GT, Guatemala
                    case ExchangeMic.Xgtg:
                    // SV, Salvador
                    case ExchangeMic.Xsva:
                    // CR, Costa Rica
                    case ExchangeMic.Xbnv:
                        return new TimeSpan(-6, 0, 0);
                    // VU, Vanuatu
                    case ExchangeMic.Gxma:
                        return new TimeSpan(-11, 0, 0);
                    // GB, United Kingdom
                    case ExchangeMic.Amts:
                    case ExchangeMic.Balt:
                    case ExchangeMic.Bate:
                    case ExchangeMic.Bgci:
                    case ExchangeMic.Blkx:
                    case ExchangeMic.Blnk:
                    case ExchangeMic.Blox:
                    case ExchangeMic.Boat:
                    case ExchangeMic.Bosc:
                    case ExchangeMic.Btee:
                    case ExchangeMic.Caze:
                    case ExchangeMic.Cco2:
                    case ExchangeMic.Cgme:
                    case ExchangeMic.Chix:
                    case ExchangeMic.Chiy:
                    case ExchangeMic.Cmec:
                    case ExchangeMic.Cmts:
                    case ExchangeMic.Cxrt:
                    case ExchangeMic.Emts:
                    case ExchangeMic.Fair:
                    case ExchangeMic.Gemx:
                    case ExchangeMic.Gfia:
                    case ExchangeMic.Gfic:
                    case ExchangeMic.Gfif:
                    case ExchangeMic.Gfim:
                    case ExchangeMic.Gfin:
                    case ExchangeMic.Gfir:
                    case ExchangeMic.Gmts:
                    case ExchangeMic.Hung:
                    case ExchangeMic.Icah:
                    case ExchangeMic.Icap:
                    case ExchangeMic.Icen:
                    case ExchangeMic.Icse:
                    case ExchangeMic.Ictq:
                    case ExchangeMic.Ifeu:
                    case ExchangeMic.Imts:
                    case ExchangeMic.Iswa:
                    case ExchangeMic.Kleu:
                    case ExchangeMic.Liqh:
                    case ExchangeMic.Liqu:
                    case ExchangeMic.Lmad:
                    case ExchangeMic.Lmae:
                    case ExchangeMic.Lmaf:
                    case ExchangeMic.Lmao:
                    case ExchangeMic.Lmax:
                    case ExchangeMic.Lmts:
                    case ExchangeMic.Lppm:
                    case ExchangeMic.Mael:
                    case ExchangeMic.Mczk:
                    case ExchangeMic.Mfgl:
                    case ExchangeMic.Mtsa:
                    case ExchangeMic.Mtsg:
                    case ExchangeMic.Mtss:
                    case ExchangeMic.Mytr:
                    case ExchangeMic.N2Ex:
                    case ExchangeMic.Nave:
                    case ExchangeMic.Nmts:
                    case ExchangeMic.Noff:
                    case ExchangeMic.Nurd:
                    case ExchangeMic.Nuro:
                    case ExchangeMic.Nxeu:
                    case ExchangeMic.Oilx:
                    case ExchangeMic.Pieu:
                    case ExchangeMic.Pirm:
                    case ExchangeMic.Pldx:
                    case ExchangeMic.Plsx:
                    case ExchangeMic.Prme:
                    case ExchangeMic.Qwix:
                    case ExchangeMic.Rbsx:
                    case ExchangeMic.Rmts:
                    case ExchangeMic.Rtsl:
                    case ExchangeMic.Secf:
                    case ExchangeMic.Sgmx:
                    case ExchangeMic.Shar:
                    case ExchangeMic.Smts:
                    case ExchangeMic.Spec:
                    case ExchangeMic.Sprz:
                    case ExchangeMic.Swap:
                    case ExchangeMic.Tben:
                    case ExchangeMic.Tbla:
                    case ExchangeMic.Tcds:
                    case ExchangeMic.Tfsc:
                    case ExchangeMic.Tfse:
                    case ExchangeMic.Tfsg:
                    case ExchangeMic.Tfss:
                    case ExchangeMic.Tfsv:
                    case ExchangeMic.Tmts:
                    case ExchangeMic.Tpcd:
                    case ExchangeMic.Tpfd:
                    case ExchangeMic.Tpie:
                    case ExchangeMic.Tpim:
                    case ExchangeMic.Tpre:
                    case ExchangeMic.Tpsd:
                    case ExchangeMic.Trde:
                    case ExchangeMic.Trdx:
                    case ExchangeMic.Treu:
                    case ExchangeMic.Trqd:
                    case ExchangeMic.Trqm:
                    case ExchangeMic.Trqx:
                    case ExchangeMic.Ukgd:
                    case ExchangeMic.Ukpx:
                    case ExchangeMic.Vega:
                    case ExchangeMic.Vmts:
                    case ExchangeMic.Wclk:
                    case ExchangeMic.Xalt:
                    case ExchangeMic.Xcor:
                    case ExchangeMic.Xgfi:
                    case ExchangeMic.Xlbm:
                    case ExchangeMic.Xldn:
                    case ExchangeMic.Xlif:
                    case ExchangeMic.Xlme:
                    case ExchangeMic.Xlon:
                    case ExchangeMic.Xplu:
                    case ExchangeMic.Xsmp:
                    case ExchangeMic.Xswb:
                    case ExchangeMic.Xtpe:
                    case ExchangeMic.Xubs:
                    // IE, Ireland
                    case ExchangeMic.Xcde:
                    case ExchangeMic.Xdub:
                    case ExchangeMic.Xeye:
                    case ExchangeMic.Xpos:
                    // IS, Iceland
                    case ExchangeMic.Isec:
                    case ExchangeMic.Xice:
                    // GH, Ghana
                    case ExchangeMic.Xgha:
                    // MA, Morocco
                    case ExchangeMic.Xcas:
                    // GG, Channel Islands
                    case ExchangeMic.Xcie:
                    // PT, Portugal
                    case ExchangeMic.Mdip:
                    case ExchangeMic.Omip:
                    case ExchangeMic.Opex:
                        return new TimeSpan(0, 0, 0);
                    // PT, Portugal
                    case ExchangeMic.Alxl:
                    case ExchangeMic.Enxl:
                    case ExchangeMic.Mfox:
                    case ExchangeMic.Wqxl:
                    case ExchangeMic.Xlis:
                        return new TimeSpan(1, 0, 0);
                }
                return new TimeSpan(0L);
            }
        }
        #endregion

        #region EuronextMep
        /// <summary>
        /// The NYSE Euronext Market Entry Place (MEP) symbol of this exchange.
        /// </summary>
        public string EuronextMep
        {
            get { return MicToEuronextMep(mic); }
        }
        #endregion

        #region EuronextMepNumber
        /// <summary>
        /// The NYSE Euronext Market Entry Place (MEP) number for this exchange.
        /// </summary>
        public int EuronextMepNumber
        {
            get { return MicToEuronextMepNumber(mic); }
        }
        #endregion

        #region IsEuronext
        /// <summary>
        /// Is this exchange belongs to the NYSE Euronext family.
        /// </summary>
        public bool IsEuronext
        {
            get { return IsEuronextMic(mic); }
        }
        #endregion

        #region Country
        /// <summary>
        /// The country.
        /// </summary>
        public ExchangeCountry Country
        {
            get
            {
                switch (mic)
                {
                    case ExchangeMic.Xtir:
                        return ExchangeCountry.Albania;
                    case ExchangeMic.Xalg:
                        return ExchangeCountry.Algeria;
                    case ExchangeMic.Bace:
                    case ExchangeMic.Bcfs:
                    case ExchangeMic.Mvcx:
                    case ExchangeMic.Rofx:
                    case ExchangeMic.Xbcc:
                    case ExchangeMic.Xbcm:
                    case ExchangeMic.Xbcx:
                    case ExchangeMic.Xbue:
                    case ExchangeMic.Xcnf:
                    case ExchangeMic.Xmab:
                    case ExchangeMic.Xmev:
                    case ExchangeMic.Xmtb:
                    case ExchangeMic.Xmvl:
                    case ExchangeMic.Xros:
                    case ExchangeMic.Xrox:
                    case ExchangeMic.Xtuc:
                        return ExchangeCountry.Argentina;
                    case ExchangeMic.Xarm:
                        return ExchangeCountry.Armenia;
                    case ExchangeMic.Apxl:
                    case ExchangeMic.Asxc:
                    case ExchangeMic.Asxp:
                    case ExchangeMic.Asxv:
                    case ExchangeMic.Awbx:
                    case ExchangeMic.Awex:
                    case ExchangeMic.Chia:
                    case ExchangeMic.Maqx:
                    case ExchangeMic.Meau:
                    case ExchangeMic.Nsxb:
                    case ExchangeMic.Siga:
                    case ExchangeMic.Simv:
                    case ExchangeMic.Xasx:
                    case ExchangeMic.Xnec:
                    case ExchangeMic.Xsfe:
                    case ExchangeMic.Xyie:
                        return ExchangeCountry.Australia;
                    case ExchangeMic.Exaa:
                    case ExchangeMic.Wbah:
                    case ExchangeMic.Wbdm:
                    case ExchangeMic.Wbgf:
                    case ExchangeMic.Xceg:
                    case ExchangeMic.Xvie:
                    case ExchangeMic.Xwbo:
                        return ExchangeCountry.Austria;
                    case ExchangeMic.Bsex:
                    case ExchangeMic.Xibe:
                        return ExchangeCountry.Azerbaijan;
                    case ExchangeMic.Xbaa:
                        return ExchangeCountry.Bahamas;
                    case ExchangeMic.Bfex:
                    case ExchangeMic.Xbah:
                        return ExchangeCountry.Bahrain;
                    case ExchangeMic.Xchg:
                    case ExchangeMic.Xdha:
                        return ExchangeCountry.Bangladesh;
                    case ExchangeMic.Xbab:
                        return ExchangeCountry.Barbados;
                    case ExchangeMic.Bcse:
                        return ExchangeCountry.Belarus;
                    case ExchangeMic.Alxb:
                    case ExchangeMic.Blpx:
                    case ExchangeMic.Bmts:
                    case ExchangeMic.Enxb:
                    case ExchangeMic.Frrf:
                    case ExchangeMic.Mlxb:
                    case ExchangeMic.Mtsd:
                    case ExchangeMic.Mtsf:
                    case ExchangeMic.Tnlb:
                    case ExchangeMic.Vpxb:
                    case ExchangeMic.Xbrd:
                    case ExchangeMic.Xbru:
                        return ExchangeCountry.Belgium;
                    case ExchangeMic.Xbda:
                        return ExchangeCountry.Bermuda;
                    case ExchangeMic.Xbol:
                        return ExchangeCountry.Bolivia;
                    case ExchangeMic.Xblb:
                    case ExchangeMic.Xsse:
                        return ExchangeCountry.BosniaHerzegovina;
                    case ExchangeMic.Xbot:
                        return ExchangeCountry.Botswana;
                    case ExchangeMic.Bcmm:
                    case ExchangeMic.Bovm:
                    case ExchangeMic.Brix:
                    case ExchangeMic.Bvmf:
                    case ExchangeMic.Ceti:
                    case ExchangeMic.Selc:
                        return ExchangeCountry.Brazil;
                    case ExchangeMic.Xbul:
                        return ExchangeCountry.Bulgaria;
                    case ExchangeMic.Xdsx:
                        return ExchangeCountry.Cameroon;
                    case ExchangeMic.Atsa:
                    case ExchangeMic.Canx:
                    case ExchangeMic.Chic:
                    case ExchangeMic.Ifca:
                    case ExchangeMic.Lica:
                    case ExchangeMic.Matn:
                    case ExchangeMic.Ngxc:
                    case ExchangeMic.Omga:
                    case ExchangeMic.Pure:
                    case ExchangeMic.Tmxs:
                    case ExchangeMic.Xats:
                    case ExchangeMic.Xbbk:
                    case ExchangeMic.Xcnq:
                    case ExchangeMic.Xicx:
                    case ExchangeMic.Xmoc:
                    case ExchangeMic.Xmod:
                    case ExchangeMic.Xtnx:
                    case ExchangeMic.Xtse:
                    case ExchangeMic.Xtsx:
                        return ExchangeCountry.Canada;
                    case ExchangeMic.Xbvc:
                        return ExchangeCountry.CapeVerde;
                    case ExchangeMic.Xcay:
                        return ExchangeCountry.CaymanIslands;
                    case ExchangeMic.Bova:
                    case ExchangeMic.Xbcl:
                    case ExchangeMic.Xsgo:
                        return ExchangeCountry.Chile;
                    case ExchangeMic.Ccfx:
                    case ExchangeMic.Cssx:
                    case ExchangeMic.Sgex:
                    case ExchangeMic.Xcfe:
                    case ExchangeMic.Xdce:
                    case ExchangeMic.Xsge:
                    case ExchangeMic.Xshe:
                    case ExchangeMic.Xshg:
                    case ExchangeMic.Xzce:
                        return ExchangeCountry.China;
                    case ExchangeMic.Xbog:
                        return ExchangeCountry.Colombia;
                    case ExchangeMic.Xbnv:
                        return ExchangeCountry.CostaRica;
                    case ExchangeMic.Xtrz:
                    case ExchangeMic.Xzag:
                    case ExchangeMic.Xzam:
                        return ExchangeCountry.Croatia;
                    case ExchangeMic.Dcsx:
                        return ExchangeCountry.Curacao;
                    case ExchangeMic.Xcyo:
                    case ExchangeMic.Xcys:
                    case ExchangeMic.Xecm:
                        return ExchangeCountry.Cyprus;
                    case ExchangeMic.Spad:
                    case ExchangeMic.Xpra:
                    case ExchangeMic.Xpxe:
                    case ExchangeMic.Xrmo:
                    case ExchangeMic.Xrmz:
                        return ExchangeCountry.CzechRepublic;
                    case ExchangeMic.Damp:
                    case ExchangeMic.Dktc:
                    case ExchangeMic.Gxgf:
                    case ExchangeMic.Gxgm:
                    case ExchangeMic.Npga:
                    case ExchangeMic.Xcse:
                    case ExchangeMic.Xfnd:
                        return ExchangeCountry.Denmark;
                    case ExchangeMic.Xbvr:
                        return ExchangeCountry.DominicanRepublic;
                    case ExchangeMic.Xgua:
                    case ExchangeMic.Xqui:
                        return ExchangeCountry.Ecuador;
                    case ExchangeMic.Nilx:
                    case ExchangeMic.Xcai:
                        return ExchangeCountry.Egypt;
                    case ExchangeMic.Xsva:
                        return ExchangeCountry.Salvador;
                    case ExchangeMic.Fnee:
                    case ExchangeMic.Xtal:
                        return ExchangeCountry.Estonia;
                    case ExchangeMic.Vmfx:
                        return ExchangeCountry.Faroeislands;
                    case ExchangeMic.Xsps:
                        return ExchangeCountry.Fiji;
                    case ExchangeMic.Fnfi:
                    case ExchangeMic.Xhel:
                        return ExchangeCountry.Finland;
                    case ExchangeMic.Alxp:
                    case ExchangeMic.Coal:
                    case ExchangeMic.Epex:
                    case ExchangeMic.Fmts:
                    case ExchangeMic.Gmtf:
                    case ExchangeMic.Mtch:
                    case ExchangeMic.Xafr:
                    case ExchangeMic.Xbln:
                    case ExchangeMic.Xmat:
                    case ExchangeMic.Xmli:
                    case ExchangeMic.Xmon:
                    case ExchangeMic.Xpar:
                    case ExchangeMic.Xpow:
                        return ExchangeCountry.France;
                    case ExchangeMic.Xgse:
                        return ExchangeCountry.Georgia;
                    case ExchangeMic.X360T:
                    case ExchangeMic.Bera:
                    case ExchangeMic.Berb:
                    case ExchangeMic.Berc:
                    case ExchangeMic.Cats:
                    case ExchangeMic.Dbox:
                    case ExchangeMic.Dusa:
                    case ExchangeMic.Dusb:
                    case ExchangeMic.Dusc:
                    case ExchangeMic.Dusd:
                    case ExchangeMic.Ecag:
                    case ExchangeMic.Eqta:
                    case ExchangeMic.Eqtb:
                    case ExchangeMic.Eqtc:
                    case ExchangeMic.Eqtd:
                    case ExchangeMic.Euwx:
                    case ExchangeMic.Fraa:
                    case ExchangeMic.Frab:
                    case ExchangeMic.Frad:
                    case ExchangeMic.Gmex:
                    case ExchangeMic.Hama:
                    case ExchangeMic.Hamb:
                    case ExchangeMic.Hana:
                    case ExchangeMic.Hanb:
                    case ExchangeMic.Muna:
                    case ExchangeMic.Munb:
                    case ExchangeMic.Plus:
                    case ExchangeMic.Stua:
                    case ExchangeMic.Stub:
                    case ExchangeMic.Xber:
                    case ExchangeMic.Xdbc:
                    case ExchangeMic.Xdbv:
                    case ExchangeMic.Xdbx:
                    case ExchangeMic.Xdus:
                    case ExchangeMic.Xeee:
                    case ExchangeMic.Xeqt:
                    case ExchangeMic.Xeta:
                    case ExchangeMic.Xetb:
                    case ExchangeMic.Xetc:
                    case ExchangeMic.Xetd:
                    case ExchangeMic.Xeti:
                    case ExchangeMic.Xetr:
                    case ExchangeMic.Xeub:
                    case ExchangeMic.Xeum:
                    case ExchangeMic.Xeup:
                    case ExchangeMic.Xeur:
                    case ExchangeMic.Xfra:
                    case ExchangeMic.Xgat:
                    case ExchangeMic.Xgrm:
                    case ExchangeMic.Xham:
                    case ExchangeMic.Xhan:
                    case ExchangeMic.Xinv:
                    case ExchangeMic.Xmun:
                    case ExchangeMic.Xnew:
                    case ExchangeMic.Xsc1:
                    case ExchangeMic.Xsc2:
                    case ExchangeMic.Xsc3:
                    case ExchangeMic.Xstu:
                    case ExchangeMic.Xxsc:
                    case ExchangeMic.Zobx:
                        return ExchangeCountry.Germany;
                    case ExchangeMic.Xgha:
                        return ExchangeCountry.Ghana;
                    case ExchangeMic.Enax:
                    case ExchangeMic.Euax:
                    case ExchangeMic.Hdat:
                    case ExchangeMic.Hotc:
                    case ExchangeMic.Xade:
                    case ExchangeMic.Xath:
                        return ExchangeCountry.Greece;
                    case ExchangeMic.Xgtg:
                        return ExchangeCountry.Guatemala;
                    case ExchangeMic.Xcie:
                        return ExchangeCountry.ChannelIslands;
                    case ExchangeMic.Gsci:
                        return ExchangeCountry.Guyana;
                    case ExchangeMic.Xbcv:
                        return ExchangeCountry.Honduras;
                    case ExchangeMic.Cgmh:
                    case ExchangeMic.Dbhk:
                    case ExchangeMic.Eotc:
                    case ExchangeMic.Hkme:
                    case ExchangeMic.Hsxa:
                    case ExchangeMic.Mehk:
                    case ExchangeMic.Tocp:
                    case ExchangeMic.Ubsx:
                    case ExchangeMic.Xcgs:
                    case ExchangeMic.Xgem:
                    case ExchangeMic.Xhkf:
                    case ExchangeMic.Xhkg:
                    case ExchangeMic.Xihk:
                    case ExchangeMic.Xpst:
                    case ExchangeMic.Sigh:
                        return ExchangeCountry.HongKong;
                    case ExchangeMic.Beta:
                    case ExchangeMic.Hupx:
                    case ExchangeMic.Qmtf:
                    case ExchangeMic.Xbud:
                        return ExchangeCountry.Hungary;
                    case ExchangeMic.Isec:
                    case ExchangeMic.Xice:
                        return ExchangeCountry.Iceland;
                    case ExchangeMic.Acex:
                    case ExchangeMic.Bsme:
                    case ExchangeMic.Icxl:
                    case ExchangeMic.Isex:
                    case ExchangeMic.Mcxx:
                    case ExchangeMic.Nbot:
                    case ExchangeMic.Nmce:
                    case ExchangeMic.Otcx:
                    case ExchangeMic.Pxil:
                    case ExchangeMic.Xban:
                    case ExchangeMic.Xbom:
                    case ExchangeMic.Xcal:
                    case ExchangeMic.Xdes:
                    case ExchangeMic.Ximc:
                    case ExchangeMic.Xmds:
                    case ExchangeMic.Xncd:
                    case ExchangeMic.Xnse:
                    case ExchangeMic.Xuse:
                        return ExchangeCountry.India;
                    case ExchangeMic.Icdx:
                    case ExchangeMic.Xbbj:
                    case ExchangeMic.Xidx:
                    case ExchangeMic.Xjnb:
                        return ExchangeCountry.Indonesia;
                    case ExchangeMic.Imex:
                    case ExchangeMic.Xteh:
                        return ExchangeCountry.Iran;
                    case ExchangeMic.Xiqs:
                        return ExchangeCountry.Iraq;
                    case ExchangeMic.Xcde:
                    case ExchangeMic.Xdub:
                    case ExchangeMic.Xeye:
                    case ExchangeMic.Xpos:
                        return ExchangeCountry.Ireland;
                    case ExchangeMic.Xtae:
                        return ExchangeCountry.Israel;
                    case ExchangeMic.Bond:
                    case ExchangeMic.Emdr:
                    case ExchangeMic.Emid:
                    case ExchangeMic.Emir:
                    case ExchangeMic.Etfp:
                    case ExchangeMic.Etlx:
                    case ExchangeMic.Hmod:
                    case ExchangeMic.Hmtf:
                    case ExchangeMic.Macx:
                    case ExchangeMic.Mivx:
                    case ExchangeMic.Motx:
                    case ExchangeMic.Mtaa:
                    case ExchangeMic.Mtsc:
                    case ExchangeMic.Mtsm:
                    case ExchangeMic.Sedx:
                    case ExchangeMic.Ssob:
                    case ExchangeMic.Xaim:
                    case ExchangeMic.Xdmi:
                    case ExchangeMic.Xgme:
                    case ExchangeMic.Xmot:
                        return ExchangeCountry.Italy;
                    case ExchangeMic.Xbrv:
                        return ExchangeCountry.IvoryCoast;
                    case ExchangeMic.Xjam:
                        return ExchangeCountry.Jamaica;
                    case ExchangeMic.Chij:
                    case ExchangeMic.Citd:
                    case ExchangeMic.Citx:
                    case ExchangeMic.Jasr:
                    case ExchangeMic.Kabu:
                    case ExchangeMic.Sbij:
                    case ExchangeMic.Sigj:
                    case ExchangeMic.Vkab:
                    case ExchangeMic.Xfka:
                    case ExchangeMic.Xijp:
                    case ExchangeMic.Xjas:
                    case ExchangeMic.Xkac:
                    case ExchangeMic.Xngo:
                    case ExchangeMic.Xnks:
                    case ExchangeMic.Xose:
                    case ExchangeMic.Xosj:
                    case ExchangeMic.Xsap:
                    case ExchangeMic.Xsbi:
                    case ExchangeMic.Xtam:
                    case ExchangeMic.Xtff:
                    case ExchangeMic.Xtk1:
                    case ExchangeMic.Xtk2:
                    case ExchangeMic.Xtk3:
                    case ExchangeMic.Xtko:
                    case ExchangeMic.Xtks:
                    case ExchangeMic.Xtkt:
                        return ExchangeCountry.Japan;
                    case ExchangeMic.Xamm:
                        return ExchangeCountry.Jordan;
                    case ExchangeMic.Etsc:
                    case ExchangeMic.Xkaz:
                        return ExchangeCountry.Kazakhstan;
                    case ExchangeMic.Xnai:
                        return ExchangeCountry.Kenya;
                    case ExchangeMic.Xkfb:
                    case ExchangeMic.Xkfe:
                    case ExchangeMic.Xkos:
                    case ExchangeMic.Xkrx:
                        return ExchangeCountry.Korea;
                    case ExchangeMic.Xkuw:
                        return ExchangeCountry.Kuwait;
                    case ExchangeMic.Xkse:
                        return ExchangeCountry.Kyrgyzstan;
                    case ExchangeMic.Xlao:
                        return ExchangeCountry.Laos;
                    case ExchangeMic.Fnlv:
                    case ExchangeMic.Xris:
                        return ExchangeCountry.Latvia;
                    case ExchangeMic.Xbey:
                        return ExchangeCountry.Lebanon;
                    case ExchangeMic.Xlsm:
                        return ExchangeCountry.LibyanArabJamahiriya;
                    case ExchangeMic.Bapx:
                    case ExchangeMic.Fnlt:
                    case ExchangeMic.Nasb:
                    case ExchangeMic.Xlit:
                        return ExchangeCountry.Lithuania;
                    case ExchangeMic.Cclx:
                    case ExchangeMic.Emtf:
                    case ExchangeMic.Xlux:
                    case ExchangeMic.Xves:
                        return ExchangeCountry.Luxembourg;
                    case ExchangeMic.Xmae:
                        return ExchangeCountry.Macedonia;
                    case ExchangeMic.Xmdg:
                        return ExchangeCountry.Madagascar;
                    case ExchangeMic.Xmsw:
                        return ExchangeCountry.Malawi;
                    case ExchangeMic.Mesq:
                    case ExchangeMic.Xkls:
                    case ExchangeMic.Xlfx:
                    case ExchangeMic.Xrbm:
                        return ExchangeCountry.Malaysia;
                    case ExchangeMic.Malx:
                        return ExchangeCountry.Maldives;
                    case ExchangeMic.Ewsm:
                    case ExchangeMic.Xmal:
                        return ExchangeCountry.Malta;
                    case ExchangeMic.Gbot:
                    case ExchangeMic.Xmau:
                        return ExchangeCountry.Mauritius;
                    case ExchangeMic.Cgmx:
                    case ExchangeMic.Xemd:
                    case ExchangeMic.Xmex:
                        return ExchangeCountry.Mexico;
                    case ExchangeMic.Xmol:
                        return ExchangeCountry.Moldova;
                    case ExchangeMic.Xula:
                        return ExchangeCountry.Mongolia;
                    case ExchangeMic.Xmnx:
                        return ExchangeCountry.Montenegro;
                    case ExchangeMic.Xcas:
                        return ExchangeCountry.Morocco;
                    case ExchangeMic.Xmap:
                        return ExchangeCountry.Mozambique;
                    case ExchangeMic.Xnam:
                        return ExchangeCountry.Namibia;
                    case ExchangeMic.Xnep:
                        return ExchangeCountry.Nepal;
                    case ExchangeMic.Nzfx:
                    case ExchangeMic.Xnze:
                        return ExchangeCountry.NewZealand;
                    case ExchangeMic.Xman:
                        return ExchangeCountry.Nicaragua;
                    case ExchangeMic.Xnsa:
                        return ExchangeCountry.Nigeria;
                    case ExchangeMic.Fish:
                    case ExchangeMic.Fshx:
                    case ExchangeMic.Icas:
                    case ExchangeMic.Nops:
                    case ExchangeMic.Norx:
                    case ExchangeMic.Notc:
                    case ExchangeMic.Xima:
                    case ExchangeMic.Xoam:
                    case ExchangeMic.Xoas:
                    case ExchangeMic.Xosl:
                        return ExchangeCountry.Norway;
                    case ExchangeMic.Xmus:
                        return ExchangeCountry.Oman;
                    case ExchangeMic.Ncel:
                    case ExchangeMic.Xisl:
                    case ExchangeMic.Xkar:
                    case ExchangeMic.Xlah:
                        return ExchangeCountry.Pakistan;
                    case ExchangeMic.Xpae:
                        return ExchangeCountry.PalestinianTerritory;
                    case ExchangeMic.Xpty:
                        return ExchangeCountry.Panama;
                    case ExchangeMic.Xpom:
                        return ExchangeCountry.PapuaNewGuinea;
                    case ExchangeMic.Xvpa:
                        return ExchangeCountry.Paraguay;
                    case ExchangeMic.Xlim:
                        return ExchangeCountry.Peru;
                    case ExchangeMic.Pdex:
                    case ExchangeMic.Xphs:
                        return ExchangeCountry.Philippines;
                    case ExchangeMic.Bosp:
                    case ExchangeMic.Mtsp:
                    case ExchangeMic.Plpx:
                    case ExchangeMic.Poee:
                    case ExchangeMic.Rpwc:
                    case ExchangeMic.Tbsp:
                    case ExchangeMic.Xnco:
                    case ExchangeMic.Xwar:
                        return ExchangeCountry.Poland;
                    case ExchangeMic.Alxl:
                    case ExchangeMic.Enxl:
                    case ExchangeMic.Mdip:
                    case ExchangeMic.Mfox:
                    case ExchangeMic.Omip:
                    case ExchangeMic.Opex:
                    case ExchangeMic.Wqxl:
                    case ExchangeMic.Xlis:
                        return ExchangeCountry.Portugal;
                    case ExchangeMic.Dsmd:
                        return ExchangeCountry.Qatar;
                    case ExchangeMic.Bmfa:
                    case ExchangeMic.Bmfm:
                    case ExchangeMic.Bmfx:
                    case ExchangeMic.Sbmf:
                    case ExchangeMic.Xbrm:
                    case ExchangeMic.Xbsd:
                    case ExchangeMic.Xbse:
                    case ExchangeMic.Xcan:
                    case ExchangeMic.Xras:
                    case ExchangeMic.Xrpm:
                        return ExchangeCountry.Romania;
                    case ExchangeMic.Ixsp:
                    case ExchangeMic.Misx:
                    case ExchangeMic.Namx:
                    case ExchangeMic.Nncs:
                    case ExchangeMic.Rpdx:
                    case ExchangeMic.Rtsx:
                    case ExchangeMic.Spim:
                    case ExchangeMic.Xapi:
                    case ExchangeMic.Xmos:
                    case ExchangeMic.Xpet:
                    case ExchangeMic.Xpic:
                    case ExchangeMic.Xrus:
                    case ExchangeMic.Xsam:
                    case ExchangeMic.Xsib:
                        return ExchangeCountry.Russia;
                    case ExchangeMic.Rotc:
                    case ExchangeMic.Rsex:
                        return ExchangeCountry.Rwanda;
                    case ExchangeMic.Xecs:
                        return ExchangeCountry.SaintKittsNevis;
                    case ExchangeMic.Xsau:
                        return ExchangeCountry.SaudiArabia;
                    case ExchangeMic.Xbel:
                        return ExchangeCountry.Serbia;
                    case ExchangeMic.Chie:
                    case ExchangeMic.Cltd:
                    case ExchangeMic.Jadx:
                    case ExchangeMic.Smex:
                    case ExchangeMic.Tfsa:
                    case ExchangeMic.Xsca:
                    case ExchangeMic.Xsce:
                    case ExchangeMic.Xscl:
                    case ExchangeMic.Xses:
                    case ExchangeMic.Xsim:
                        return ExchangeCountry.Singapore;
                    case ExchangeMic.Xbra:
                        return ExchangeCountry.Slovakia;
                    case ExchangeMic.Xlju:
                    case ExchangeMic.Xsop:
                        return ExchangeCountry.Slovenia;
                    case ExchangeMic.Altx:
                    case ExchangeMic.Xbes:
                    case ExchangeMic.Xjse:
                    case ExchangeMic.Xsaf:
                    case ExchangeMic.Xsfa:
                    case ExchangeMic.Yldx:
                        return ExchangeCountry.SouthAfrica;
                    case ExchangeMic.Mabx:
                    case ExchangeMic.Omel:
                    case ExchangeMic.Pave:
                    case ExchangeMic.Send:
                    case ExchangeMic.Xbar:
                    case ExchangeMic.Xbil:
                    case ExchangeMic.Xdpa:
                    case ExchangeMic.Xdrf:
                    case ExchangeMic.Xlat:
                    case ExchangeMic.Xmad:
                    case ExchangeMic.Xmce:
                    case ExchangeMic.Xmef:
                    case ExchangeMic.Xmrv:
                    case ExchangeMic.Xnaf:
                    case ExchangeMic.Xsrm:
                    case ExchangeMic.Xval:
                        return ExchangeCountry.Spain;
                    case ExchangeMic.Xcol:
                        return ExchangeCountry.SriLanka;
                    case ExchangeMic.Xkha:
                        return ExchangeCountry.Sudan;
                    case ExchangeMic.Xswa:
                        return ExchangeCountry.Swaziland;
                    case ExchangeMic.Burg:
                    case ExchangeMic.Burm:
                    case ExchangeMic.Fnse:
                    case ExchangeMic.Nmtf:
                    case ExchangeMic.Xndx:
                    case ExchangeMic.Xngm:
                    case ExchangeMic.Xnmr:
                    case ExchangeMic.Xopv:
                    case ExchangeMic.Xsat:
                    case ExchangeMic.Xsto:
                        return ExchangeCountry.Sweden;
                    case ExchangeMic.Xbrn:
                    case ExchangeMic.Xqmh:
                    case ExchangeMic.Xscu:
                    case ExchangeMic.Xstv:
                    case ExchangeMic.Xstx:
                    case ExchangeMic.Xswx:
                    case ExchangeMic.Xvtx:
                    case ExchangeMic.Zkbx:
                        return ExchangeCountry.Switzerland;
                    case ExchangeMic.Xdse:
                        return ExchangeCountry.Syria;
                    case ExchangeMic.Roco:
                    case ExchangeMic.Xtaf:
                    case ExchangeMic.Xtai:
                        return ExchangeCountry.Taiwan;
                    case ExchangeMic.Xdar:
                        return ExchangeCountry.Tanzania;
                    case ExchangeMic.Afet:
                    case ExchangeMic.Beex:
                    case ExchangeMic.Tfex:
                    case ExchangeMic.Xbkf:
                    case ExchangeMic.Xbkk:
                    case ExchangeMic.Xmai:
                        return ExchangeCountry.Thailand;
                    case ExchangeMic.Alxa:
                    case ExchangeMic.Clmx:
                    case ExchangeMic.Ecxe:
                    case ExchangeMic.Ndex:
                    case ExchangeMic.Nlpx:
                    case ExchangeMic.Tnla:
                    case ExchangeMic.Tomd:
                    case ExchangeMic.Tomx:
                    case ExchangeMic.Xams:
                    case ExchangeMic.Xeuc:
                    case ExchangeMic.Xeue:
                    case ExchangeMic.Xeui:
                    case ExchangeMic.Xhft:
                        return ExchangeCountry.Netherlands;
                    case ExchangeMic.Xtrn:
                        return ExchangeCountry.TrinidadTobago;
                    case ExchangeMic.Xtun:
                        return ExchangeCountry.Tunisia;
                    case ExchangeMic.Xiab:
                    case ExchangeMic.Xist:
                    case ExchangeMic.Xtur:
                        return ExchangeCountry.Turkey;
                    case ExchangeMic.Xuga:
                        return ExchangeCountry.Uganda;
                    case ExchangeMic.Eese:
                    case ExchangeMic.Pftq:
                    case ExchangeMic.Pfts:
                    case ExchangeMic.Sepe:
                    case ExchangeMic.Ukex:
                    case ExchangeMic.Xdfb:
                    case ExchangeMic.Xkhr:
                    case ExchangeMic.Xkie:
                    case ExchangeMic.Xkis:
                    case ExchangeMic.Xode:
                    case ExchangeMic.Xpri:
                    case ExchangeMic.Xuax:
                    case ExchangeMic.Xukr:
                        return ExchangeCountry.Ukraine;
                    case ExchangeMic.Dgcx:
                    case ExchangeMic.Difx:
                    case ExchangeMic.Dumx:
                    case ExchangeMic.Xads:
                    case ExchangeMic.Xdfm:
                        return ExchangeCountry.ArabEmirates;
                    case ExchangeMic.Amts:
                    case ExchangeMic.Balt:
                    case ExchangeMic.Bate:
                    case ExchangeMic.Bgci:
                    case ExchangeMic.Blkx:
                    case ExchangeMic.Blnk:
                    case ExchangeMic.Blox:
                    case ExchangeMic.Boat:
                    case ExchangeMic.Bosc:
                    case ExchangeMic.Btee:
                    case ExchangeMic.Caze:
                    case ExchangeMic.Cco2:
                    case ExchangeMic.Cgme:
                    case ExchangeMic.Chix:
                    case ExchangeMic.Chiy:
                    case ExchangeMic.Cmec:
                    case ExchangeMic.Cmts:
                    case ExchangeMic.Cxrt:
                    case ExchangeMic.Emts:
                    case ExchangeMic.Fair:
                    case ExchangeMic.Gemx:
                    case ExchangeMic.Gfia:
                    case ExchangeMic.Gfic:
                    case ExchangeMic.Gfif:
                    case ExchangeMic.Gfim:
                    case ExchangeMic.Gfin:
                    case ExchangeMic.Gfir:
                    case ExchangeMic.Gmts:
                    case ExchangeMic.Hung:
                    case ExchangeMic.Icah:
                    case ExchangeMic.Icap:
                    case ExchangeMic.Icen:
                    case ExchangeMic.Icse:
                    case ExchangeMic.Ictq:
                    case ExchangeMic.Ifeu:
                    case ExchangeMic.Imts:
                    case ExchangeMic.Iswa:
                    case ExchangeMic.Kleu:
                    case ExchangeMic.Liqh:
                    case ExchangeMic.Liqu:
                    case ExchangeMic.Lmad:
                    case ExchangeMic.Lmae:
                    case ExchangeMic.Lmaf:
                    case ExchangeMic.Lmao:
                    case ExchangeMic.Lmax:
                    case ExchangeMic.Lmts:
                    case ExchangeMic.Lppm:
                    case ExchangeMic.Mael:
                    case ExchangeMic.Mczk:
                    case ExchangeMic.Mfgl:
                    case ExchangeMic.Mtsa:
                    case ExchangeMic.Mtsg:
                    case ExchangeMic.Mtss:
                    case ExchangeMic.Mytr:
                    case ExchangeMic.N2Ex:
                    case ExchangeMic.Nave:
                    case ExchangeMic.Nmts:
                    case ExchangeMic.Noff:
                    case ExchangeMic.Nurd:
                    case ExchangeMic.Nuro:
                    case ExchangeMic.Nxeu:
                    case ExchangeMic.Oilx:
                    case ExchangeMic.Pieu:
                    case ExchangeMic.Pirm:
                    case ExchangeMic.Pldx:
                    case ExchangeMic.Plsx:
                    case ExchangeMic.Prme:
                    case ExchangeMic.Qwix:
                    case ExchangeMic.Rbsx:
                    case ExchangeMic.Rmts:
                    case ExchangeMic.Rtsl:
                    case ExchangeMic.Secf:
                    case ExchangeMic.Sgmx:
                    case ExchangeMic.Shar:
                    case ExchangeMic.Smts:
                    case ExchangeMic.Spec:
                    case ExchangeMic.Sprz:
                    case ExchangeMic.Swap:
                    case ExchangeMic.Tben:
                    case ExchangeMic.Tbla:
                    case ExchangeMic.Tcds:
                    case ExchangeMic.Tfsc:
                    case ExchangeMic.Tfse:
                    case ExchangeMic.Tfsg:
                    case ExchangeMic.Tfss:
                    case ExchangeMic.Tfsv:
                    case ExchangeMic.Tmts:
                    case ExchangeMic.Tpcd:
                    case ExchangeMic.Tpfd:
                    case ExchangeMic.Tpie:
                    case ExchangeMic.Tpim:
                    case ExchangeMic.Tpre:
                    case ExchangeMic.Tpsd:
                    case ExchangeMic.Trde:
                    case ExchangeMic.Trdx:
                    case ExchangeMic.Treu:
                    case ExchangeMic.Trqd:
                    case ExchangeMic.Trqm:
                    case ExchangeMic.Trqx:
                    case ExchangeMic.Ukgd:
                    case ExchangeMic.Ukpx:
                    case ExchangeMic.Vega:
                    case ExchangeMic.Vmts:
                    case ExchangeMic.Wclk:
                    case ExchangeMic.Xalt:
                    case ExchangeMic.Xcor:
                    case ExchangeMic.Xgfi:
                    case ExchangeMic.Xlbm:
                    case ExchangeMic.Xldn:
                    case ExchangeMic.Xlif:
                    case ExchangeMic.Xlme:
                    case ExchangeMic.Xlon:
                    case ExchangeMic.Xplu:
                    case ExchangeMic.Xsmp:
                    case ExchangeMic.Xswb:
                    case ExchangeMic.Xtpe:
                    case ExchangeMic.Xubs:
                        return ExchangeCountry.UnitedKingdom;
                    case ExchangeMic.Aats:
                    case ExchangeMic.Aldp:
                    case ExchangeMic.Amxo:
                    case ExchangeMic.Aqua:
                    case ExchangeMic.Arcd:
                    case ExchangeMic.Arco:
                    case ExchangeMic.Arcx:
                    case ExchangeMic.Baml:
                    case ExchangeMic.Bard:
                    case ExchangeMic.Barx:
                    case ExchangeMic.Bato:
                    case ExchangeMic.Bats:
                    case ExchangeMic.Baty:
                    case ExchangeMic.Bgcf:
                    case ExchangeMic.Bids:
                    case ExchangeMic.Bltd:
                    case ExchangeMic.Bndd:
                    case ExchangeMic.Bosd:
                    case ExchangeMic.Btec:
                    case ExchangeMic.C2Ox:
                    case ExchangeMic.Caes:
                    case ExchangeMic.Cbsx:
                    case ExchangeMic.Ccfe:
                    case ExchangeMic.Cded:
                    case ExchangeMic.Cdel:
                    case ExchangeMic.Cgmi:
                    case ExchangeMic.Cgmu:
                    case ExchangeMic.Cicx:
                    case ExchangeMic.Cslp:
                    case ExchangeMic.Dbsx:
                    case ExchangeMic.Deal:
                    case ExchangeMic.Eddp:
                    case ExchangeMic.Edga:
                    case ExchangeMic.Edgd:
                    case ExchangeMic.Edgx:
                    case ExchangeMic.Eris:
                    case ExchangeMic.Fcbt:
                    case ExchangeMic.Fcme:
                    case ExchangeMic.Finn:
                    case ExchangeMic.Fino:
                    case ExchangeMic.Finr:
                    case ExchangeMic.Finy:
                    case ExchangeMic.Fxal:
                    case ExchangeMic.Fxcm:
                    case ExchangeMic.Glbx:
                    case ExchangeMic.Gllc:
                    case ExchangeMic.Govx:
                    case ExchangeMic.Gree:
                    case ExchangeMic.Gtco:
                    case ExchangeMic.Hegx:
                    case ExchangeMic.Hsfx:
                    case ExchangeMic.Iblx:
                    case ExchangeMic.Icbx:
                    case ExchangeMic.Icel:
                    case ExchangeMic.Icro:
                    case ExchangeMic.Iepa:
                    case ExchangeMic.Ifus:
                    case ExchangeMic.Iidx:
                    case ExchangeMic.Imag:
                    case ExchangeMic.Imbd:
                    case ExchangeMic.Imco:
                    case ExchangeMic.Imcr:
                    case ExchangeMic.Imen:
                    case ExchangeMic.Imeq:
                    case ExchangeMic.Imfx:
                    case ExchangeMic.Imir:
                    case ExchangeMic.Itgi:
                    case ExchangeMic.Jpmx:
                    case ExchangeMic.Kncm:
                    case ExchangeMic.Knem:
                    case ExchangeMic.Knli:
                    case ExchangeMic.Knmx:
                    case ExchangeMic.Lafd:
                    case ExchangeMic.Lafl:
                    case ExchangeMic.Lafx:
                    case ExchangeMic.Levl:
                    case ExchangeMic.Mspl:
                    case ExchangeMic.Msrp:
                    case ExchangeMic.Mstc:
                    case ExchangeMic.Nasd:
                    case ExchangeMic.Nfsa:
                    case ExchangeMic.Nfsc:
                    case ExchangeMic.Nfsd:
                    case ExchangeMic.Nodx:
                    case ExchangeMic.Nxus:
                    case ExchangeMic.Nyfx:
                    case ExchangeMic.Nypc:
                    case ExchangeMic.Nysd:
                    case ExchangeMic.Opra:
                    case ExchangeMic.Otcb:
                    case ExchangeMic.Otcq:
                    case ExchangeMic.Pdqd:
                    case ExchangeMic.Pdqx:
                    case ExchangeMic.Pinx:
                    case ExchangeMic.Pipe:
                    case ExchangeMic.Prse:
                    case ExchangeMic.Psgm:
                    case ExchangeMic.Pulx:
                    case ExchangeMic.Ricd:
                    case ExchangeMic.Ricx:
                    case ExchangeMic.Sgma:
                    case ExchangeMic.Shad:
                    case ExchangeMic.Shaw:
                    case ExchangeMic.Sigx:
                    case ExchangeMic.Sstx:
                    case ExchangeMic.Tfsu:
                    case ExchangeMic.Trck:
                    case ExchangeMic.Trfx:
                    case ExchangeMic.Trwb:
                    case ExchangeMic.Ubsa:
                    case ExchangeMic.Ubsp:
                    case ExchangeMic.Vtex:
                    case ExchangeMic.Xadf:
                    case ExchangeMic.Xaqs:
                    case ExchangeMic.Xase:
                    case ExchangeMic.Xbos:
                    case ExchangeMic.Xbox:
                    case ExchangeMic.Xbrt:
                    case ExchangeMic.Xbxo:
                    case ExchangeMic.Xcbf:
                    case ExchangeMic.Xcbo:
                    case ExchangeMic.Xcbt:
                    case ExchangeMic.Xccx:
                    case ExchangeMic.Xcec:
                    case ExchangeMic.Xcff:
                    case ExchangeMic.Xchi:
                    case ExchangeMic.Xcis:
                    case ExchangeMic.Xcme:
                    case ExchangeMic.Xcur:
                    case ExchangeMic.Xelx:
                    case ExchangeMic.Xfci:
                    case ExchangeMic.Ximm:
                    case ExchangeMic.Xiom:
                    case ExchangeMic.Xisa:
                    case ExchangeMic.Xise:
                    case ExchangeMic.Xisx:
                    case ExchangeMic.Xkbt:
                    case ExchangeMic.Xmer:
                    case ExchangeMic.Xmge:
                    case ExchangeMic.Xmio:
                    case ExchangeMic.Xnas:
                    case ExchangeMic.Xncm:
                    case ExchangeMic.Xndq:
                    case ExchangeMic.Xngs:
                    case ExchangeMic.Xnim:
                    case ExchangeMic.Xnli:
                    case ExchangeMic.Xnms:
                    case ExchangeMic.Xnye:
                    case ExchangeMic.Xnyl:
                    case ExchangeMic.Xnym:
                    case ExchangeMic.Xnys:
                    case ExchangeMic.Xoch:
                    case ExchangeMic.Xotc:
                    case ExchangeMic.Xpbt:
                    case ExchangeMic.Xphl:
                    case ExchangeMic.Xpho:
                    case ExchangeMic.Xpor:
                    case ExchangeMic.Xsef:
                    case ExchangeMic.Xwee:
                        return ExchangeCountry.UnitedStates;
                    case ExchangeMic.Bvur:
                    case ExchangeMic.Xmnt:
                        return ExchangeCountry.Uruguay;
                    case ExchangeMic.Xcet:
                    case ExchangeMic.Xcue:
                    case ExchangeMic.Xkce:
                    case ExchangeMic.Xste:
                    case ExchangeMic.Xuni:
                        return ExchangeCountry.Uzbekistan;
                    case ExchangeMic.Gxma:
                        return ExchangeCountry.Vanuatu;
                    case ExchangeMic.Bvca:
                    case ExchangeMic.Xcar:
                        return ExchangeCountry.Venezuela;
                    case ExchangeMic.Hstc:
                    case ExchangeMic.Xhnx:
                    case ExchangeMic.Xstc:
                        return ExchangeCountry.VietNam;
                    case ExchangeMic.Xlus:
                        return ExchangeCountry.Zambia;
                    case ExchangeMic.Xzim:
                        return ExchangeCountry.Zimbabwe;
                }
                return ExchangeCountry.NoCountry;
            }
        }
        #endregion
        #endregion

        #region Construction
        /// <summary>
        /// Constructs a new instance of this class.
        /// </summary>
        public Exchange()
        {
        }

        /// <summary>
        /// Constructs a new instance of this class.
        /// </summary>
        /// <param name="mic">The MIC.</param>
        public Exchange(ExchangeMic mic)
        {
            this.mic = mic;
        }

        /// <summary>
        /// Constructs a new instance of this class.
        /// </summary>
        /// <param name="euronextMic">The Euronext MIC.</param>
        public Exchange(EuronextMic euronextMic)
            : this(EuronextToMic(euronextMic))
        {
        }
        #endregion

        #region EuronextToMic
        private static ExchangeMic EuronextToMic(EuronextMic euronextMic)
        {
            switch (euronextMic)
            {
                case EuronextMic.Alxb: return ExchangeMic.Alxb;
                case EuronextMic.Enxb: return ExchangeMic.Enxb;
                case EuronextMic.Mlxb: return ExchangeMic.Mlxb;
                case EuronextMic.Tnlb: return ExchangeMic.Tnlb;
                case EuronextMic.Vpxb: return ExchangeMic.Vpxb;
                case EuronextMic.Xbrd: return ExchangeMic.Xbrd;
                case EuronextMic.Xbru: return ExchangeMic.Xbru;
                case EuronextMic.Alxp: return ExchangeMic.Alxp;
                case EuronextMic.Xmat: return ExchangeMic.Xmat;
                case EuronextMic.Xmli: return ExchangeMic.Xmli;
                case EuronextMic.Xmon: return ExchangeMic.Xmon;
                case EuronextMic.Xpar: return ExchangeMic.Xpar;
                case EuronextMic.Alxl: return ExchangeMic.Alxl;
                case EuronextMic.Enxl: return ExchangeMic.Enxl;
                case EuronextMic.Mfox: return ExchangeMic.Mfox;
                case EuronextMic.Wqxl: return ExchangeMic.Wqxl;
                case EuronextMic.Xlis: return ExchangeMic.Xlis;
                case EuronextMic.Alxa: return ExchangeMic.Alxa;
                case EuronextMic.Tnla: return ExchangeMic.Tnla;
                case EuronextMic.Xams: return ExchangeMic.Xams;
                case EuronextMic.Xeuc: return ExchangeMic.Xeuc;
                case EuronextMic.Xeue: return ExchangeMic.Xeue;
                case EuronextMic.Xeui: return ExchangeMic.Xeui;
                case EuronextMic.Xldn: return ExchangeMic.Xldn;
                case EuronextMic.Xlif: return ExchangeMic.Xlif;
                default: return ExchangeMic.Xxxx;
            }
        }
        #endregion

        #region IsEuronext
        private static bool IsEuronextMic(ExchangeMic mic)
        {
            switch (mic)
            {
                case ExchangeMic.Alxb:
                case ExchangeMic.Enxb:
                case ExchangeMic.Mlxb:
                case ExchangeMic.Tnlb:
                case ExchangeMic.Vpxb:
                case ExchangeMic.Xbrd:
                case ExchangeMic.Xbru:
                case ExchangeMic.Alxp:
                case ExchangeMic.Xmat:
                case ExchangeMic.Xmli:
                case ExchangeMic.Xmon:
                case ExchangeMic.Xpar:
                case ExchangeMic.Alxl:
                case ExchangeMic.Enxl:
                case ExchangeMic.Mfox:
                case ExchangeMic.Wqxl:
                case ExchangeMic.Xlis:
                case ExchangeMic.Alxa:
                case ExchangeMic.Tnla:
                case ExchangeMic.Xams:
                case ExchangeMic.Xeuc:
                case ExchangeMic.Xeue:
                case ExchangeMic.Xeui:
                case ExchangeMic.Xldn:
                case ExchangeMic.Xlif:
                    return true;
                default:
                    return false;
            }
        }
        #endregion

        #region MicToEuronextMep
        private static string MicToEuronextMep(ExchangeMic mic)
        {
            switch (mic)
            {
                // Should contain: Xams, Alxa, Tnla, Xeuc, Xeue, Xeui, Xhft.
                case ExchangeMic.Alxa:
                case ExchangeMic.Tnla:
                case ExchangeMic.Xams:
                case ExchangeMic.Xeuc:
                case ExchangeMic.Xeue:
                case ExchangeMic.Xeui:
                    return "AMS";

                // Should contain: Xbru, Alxb, Enxb, Mlxb, Tnlb, Vpxb, Xbrd.
                case ExchangeMic.Alxb:
                case ExchangeMic.Enxb:
                case ExchangeMic.Mlxb:
                case ExchangeMic.Tnlb:
                case ExchangeMic.Vpxb:
                case ExchangeMic.Xbrd:
                case ExchangeMic.Xbru:
                    return "BRU";

                // Should contain: Xlis, Enxl, Mfox, Wqxl.
                case ExchangeMic.Alxl:
                case ExchangeMic.Enxl:
                case ExchangeMic.Mfox:
                case ExchangeMic.Wqxl:
                case ExchangeMic.Xlis:
                    return "LIS";

                // Should contain: Xpar, Alxp, Xmli, Xmat, Xmon.
                case ExchangeMic.Alxp:
                case ExchangeMic.Xmat:
                case ExchangeMic.Xmli:
                case ExchangeMic.Xmon:
                case ExchangeMic.Xpar:
                    return "PAR";

                // Should contain: Xldn.
                //case ExchangeMic.Xldn:
                //case ExchangeMic.Xlif:
                //    return "LDN";

                default:
                    return "OTHER";
            }
        }
        #endregion

        #region MicToEuronextMepNumber
        private static int MicToEuronextMepNumber(ExchangeMic mic)
        {
            switch (mic)
            {
                // Should contain: Xams, Alxa, Tnla, Xeuc, Xeue, Xeui, Xhft.
                case ExchangeMic.Alxa:
                case ExchangeMic.Tnla:
                case ExchangeMic.Xams:
                case ExchangeMic.Xeuc:
                case ExchangeMic.Xeue:
                case ExchangeMic.Xeui:
                    return 2; // AMS

                // Should contain: Xbru, Alxb, Enxb, Mlxb, Tnlb, Vpxb, Xbrd.
                case ExchangeMic.Alxb:
                case ExchangeMic.Enxb:
                case ExchangeMic.Mlxb:
                case ExchangeMic.Tnlb:
                case ExchangeMic.Vpxb:
                case ExchangeMic.Xbrd:
                case ExchangeMic.Xbru:
                    return 3; // BRU

                // Should contain: Xlis, Enxl, Mfox, Wqxl.
                case ExchangeMic.Alxl:
                case ExchangeMic.Enxl:
                case ExchangeMic.Mfox:
                case ExchangeMic.Wqxl:
                case ExchangeMic.Xlis:
                    return 5; // LIS

                // Should contain: Xpar, Alxp, Xmli, Xmat, Xmon.
                case ExchangeMic.Alxp:
                case ExchangeMic.Xmat:
                case ExchangeMic.Xmli:
                case ExchangeMic.Xmon:
                case ExchangeMic.Xpar:
                    return 1; // PAR

                // Should contain: Xldn.
                //case ExchangeMic.Xldn:
                //case ExchangeMic.Xlif:
                //    return ?; // LDN

                default:
                    return 6; // OTHER
            }
        }
        #endregion

        #region IComparable
        /// <summary>
        /// IComparable&lt;Exchange&gt; implementation.
        /// </summary>
        /// <param name="other">The other instance to compare.</param>
        /// <returns>The result of comparison.</returns>
        public int CompareTo(Exchange other)
        {
            object obj = other;
            if (null == obj)
                return -1;
            return (int)other.mic - (int)mic;
        }
        #endregion

        #region IEquatable
        /// <summary>
        /// IEquatable&lt;Exchange&gt; implementation. Determines whether the specified instances are considered equal.
        /// </summary>
        /// <param name="other">The other instance to compare.</param>
        /// <returns>True if instances are equal, false otherwise.</returns>
        public bool Equals(Exchange other)
        {
            object obj = other;
            if (null == obj)
                return false;
            return other.mic == mic;
        }
        #endregion

        #region Overrides
        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode()
        {
            return (int)mic;
        }

        /// <summary>
        /// Determines whether the specified instances are considered equal.
        /// </summary>
        /// <param name="obj">The object to compare with this object.</param>
        /// <returns>True if objects are equal, false if not.</returns>
        public override bool Equals(object obj)
        {
            var other = obj as Exchange;
            return null == other ? false : mic == other.mic;
        }

        /// <summary>
        /// Returns the string that represents this object.
        /// </summary>
        /// <returns>Returns the string that represents this object.</returns>
        public override string ToString()
        {
            return mic.ToString();
        }

        /// <summary>
        /// The <c>==</c> operator.
        /// </summary>
        /// <param name="object1">The first object.</param>
        /// <param name="object2">The second object.</param>
        /// <returns>Boolean specifying the equality relationship.</returns>
        public static bool operator ==(Exchange object1, Exchange object2)
        {
            object obj1 = object1;
            object obj2 = object2;
            if (obj1 != null)
                return obj2 == null ? false : object1.mic == object2.mic;
            return obj2 == null;
        }

        /// <summary>
        /// The <c>!=</c> operator.
        /// </summary>
        /// <param name="object1">The first object.</param>
        /// <param name="object2">The second object.</param>
        /// <returns>Boolean specifying the inequality relationship.</returns>
        public static bool operator !=(Exchange object1, Exchange object2)
        {
            object obj1 = object1;
            object obj2 = object2;
            if (obj1 != null)
                return obj2 == null ? true : object1.mic != object2.mic;
            return obj2 != null;
        }

        /// <summary>
        /// The <c>&lt;</c> operator.
        /// </summary>
        /// <param name="object1">The first object.</param>
        /// <param name="object2">The second object.</param>
        /// <returns>Boolean specifying the less than relationship.</returns>
        public static bool operator <(Exchange object1, Exchange object2)
        {
            object obj1 = object1;
            object obj2 = object2;
            if (obj1 != null)
                return obj2 == null ? false : object1.mic < object2.mic;
            return false;
        }

        /// <summary>
        /// The <c>&gt;</c> operator.
        /// </summary>
        /// <param name="object1">The first object.</param>
        /// <param name="object2">The second object.</param>
        /// <returns>Boolean specifying the greater than relationship.</returns>
        public static bool operator >(Exchange object1, Exchange object2)
        {
            object obj1 = object1;
            object obj2 = object2;
            if (obj1 != null)
                return obj2 == null ? true : object1.mic > object2.mic;
            return false;
        }
        #endregion
    }
}
