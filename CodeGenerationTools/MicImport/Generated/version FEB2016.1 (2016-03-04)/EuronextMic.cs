using System.ComponentModel;

namespace Mbst.Trading
{
    /// <summary>
    /// NYSE Euronext (http://www.euronext.com) exchanges MIC helper.
    /// <para>Generated automatically from the list of ISO 10383 Exchange/Market Identifier Codes, version FEB2016.1 (2016-03-04).</para>
    /// <para><c>http://www.iso15022.org/MIC/homepageMIC.htm</c></para>
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", Scope = "namespace", Target = "Mbst.Trading.ExchangeMic", Justification = "Compiance with the ISO 10383 Market Identifier Code.")]
    public enum EuronextMic
    {
        /// <summary>EURONEXT - EURONEXT BRUSSELS</summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", Scope = "member", Target = "Mbst.Trading.EuronextMic.#*", Justification = "Compiance with the ISO 10383 Market Identifier Code.")]
        [Description(@"EURONEXT - EURONEXT BRUSSELS")]
        Xbru,
        /// <summary>EURONEXT - ALTERNEXT BRUSSELS</summary>
        [Description(@"EURONEXT - ALTERNEXT BRUSSELS")]
        Alxb,
        /// <summary>EURONEXT - EASY NEXT</summary>
        [Description(@"EURONEXT - EASY NEXT")]
        Enxb,
        /// <summary>EURONEXT - MARCHE LIBRE BRUSSELS</summary>
        [Description(@"EURONEXT - MARCHE LIBRE BRUSSELS")]
        Mlxb,
        /// <summary>EURONEXT - TRADING FACILITY BRUSSELS</summary>
        [Description(@"EURONEXT - TRADING FACILITY BRUSSELS")]
        Tnlb,
        /// <summary>EURONEXT - VENTES PUBLIQUES BRUSSELS</summary>
        [Description(@"EURONEXT - VENTES PUBLIQUES BRUSSELS")]
        Vpxb,
        /// <summary>EURONEXT - EURONEXT BRUSSELS - DERIVATIVES</summary>
        [Description(@"EURONEXT - EURONEXT BRUSSELS - DERIVATIVES")]
        Xbrd,
        /// <summary>EURONEXT - EURONEXT PARIS</summary>
        [Description(@"EURONEXT - EURONEXT PARIS")]
        Xpar,
        /// <summary>EURONEXT - ALTERNEXT PARIS</summary>
        [Description(@"EURONEXT - ALTERNEXT PARIS")]
        Alxp,
        /// <summary>EURONEXT PARIS MATIF</summary>
        [Description(@"EURONEXT PARIS MATIF")]
        Xmat,
        /// <summary>EURONEXT - MARCHE LIBRE PARIS</summary>
        [Description(@"EURONEXT - MARCHE LIBRE PARIS")]
        Xmli,
        /// <summary>EURONEXT PARIS MONEP</summary>
        [Description(@"EURONEXT PARIS MONEP")]
        Xmon,
        /// <summary>EURONEXT - EURONEXT LISBON</summary>
        [Description(@"EURONEXT - EURONEXT LISBON")]
        Xlis,
        /// <summary>EURONEXT - ALTERNEXT LISBON</summary>
        [Description(@"EURONEXT - ALTERNEXT LISBON")]
        Alxl,
        /// <summary>EURONEXT - EASYNEXT LISBON</summary>
        [Description(@"EURONEXT - EASYNEXT LISBON")]
        Enxl,
        /// <summary>EURONEXT - MERCADO DE FUTUROS E OPÇÕES</summary>
        [Description(@"EURONEXT - MERCADO DE FUTUROS E OPÇÕES")]
        Mfox,
        /// <summary>EURONEXT - MARKET WITHOUT QUOTATIONS LISBON</summary>
        [Description(@"EURONEXT - MARKET WITHOUT QUOTATIONS LISBON")]
        Wqxl,
        /// <summary>EURONEXT - EURONEXT AMSTERDAM</summary>
        [Description(@"EURONEXT - EURONEXT AMSTERDAM")]
        Xams,
        /// <summary>EURONEXT - ALTERNEXT AMSTERDAM</summary>
        [Description(@"EURONEXT - ALTERNEXT AMSTERDAM")]
        Alxa,
        /// <summary>EURONEXT - TRADED BUT NOT LISTED AMSTERDAM</summary>
        [Description(@"EURONEXT - TRADED BUT NOT LISTED AMSTERDAM")]
        Tnla,
        /// <summary>EURONEXT COM, COMMODITIES FUTURES AND OPTIONS</summary>
        [Description(@"EURONEXT COM, COMMODITIES FUTURES AND OPTIONS")]
        Xeuc,
        /// <summary>EURONEXT EQF, EQUITIES AND INDICES DERIVATIVES</summary>
        [Description(@"EURONEXT EQF, EQUITIES AND INDICES DERIVATIVES")]
        Xeue,
        /// <summary>EURONEXT IRF, INTEREST RATE FUTURE AND OPTIONS</summary>
        [Description(@"EURONEXT IRF, INTEREST RATE FUTURE AND OPTIONS")]
        Xeui,
        /// <summary>EURONEXT - EURONEXT LONDON</summary>
        [Description(@"EURONEXT - EURONEXT LONDON")]
        Xldn,
        /// <summary>EURONEXT OTHER OR UNKNOWN</summary>
        [Description(@"UNKNOWN")]
        Xxxx
    }
}
