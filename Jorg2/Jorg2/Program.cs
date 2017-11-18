using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Speech.Recognition;
using System.Speech.Synthesis;
using System.Globalization;
using System.Threading;
using System.Diagnostics;
using System.IO;
using System.Media;
using AIMLbot;
using System.Xml;


namespace Jorg
{
    class Program
    {
       // public static AIML bot;
        public static Process processo = null;
        public static Dictionary<string, string> dictMyMusic;
        public static SpeechRecognitionEngine engine;
        public static SpeechSynthesizer sp = null;
        public static List<string> indexes = new List<string>();// lista de noticias

        static void Main(string[] args)
        {


            //bot = new AIML();
            engine = new SpeechRecognitionEngine(new System.Globalization.CultureInfo("pt-BR"));//Instanciando Engine de Reconhecimento

            engine.SetInputToDefaultAudioDevice();//Configurando dispositivo de audio
            sp = new SpeechSynthesizer();//Instancia do sintetizador de fala

            //Vetores de Comandos
            //Musica

            //Configurando AIML

            //AIML.ConfigAIMLFiles();


            //conversas a.i
            string[] conversaAi = AIML.GetWordsOrSentences(); //PEGANDO PALAVRAS E LANÇANDO NO VETOR

            string[] mp3Files = Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic), "*.wav", SearchOption.AllDirectories);
            //Conversas
            string[] conversas = { "Jarvis", "olá", "boa noite", "boa tarde", "tudo bem", "hello", "Fala com a Isa", "café", "vou te apagar", "oi bebe", "Dá parabéns pro meu pai" };
            //Comandos do Sistemas
            string[] comandosSistema = { "apresentar", "que horas são", "que dia é hoje", "parar", "google","aulas de violão", "pesquisar", "desligar máquina","fechar aplicativo", "facebook", "fechar navegador", "youtube", "conversor mp3", "listar musicas", "listar pastas","acessar hotmail", "abrir gmail", "noticias", "listar documentos", "listar comandos","prompt de comando","ler noticias", "abrir link 1"};

            string[] documentos = Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),"",SearchOption.AllDirectories);

            String[] escolhaNews = new String[42];

     
            int i = 0;
            //Choices
            while (i<42)
            {
                escolhaNews[i] =""+i;
                i++;
            }

            Choices cNoticias = new Choices(escolhaNews);
            Choices c_conversaAi = new Choices(conversaAi);//Colocando lista de expressoes e palavras em choices
           

            Choices cMusics = new Choices();
            Choices c_conversas = new Choices(conversas);
            Choices c_comandosSistema = new Choices(comandosSistema);

            Choices c_documentos = new Choices();




            dictMyMusic = new Dictionary<string, string>();


            foreach (var mp3File in mp3Files) {
                FileInfo fi = new FileInfo(mp3File);
                String temp = fi.Name.Replace(fi.Extension, "");
                if (temp.Contains("_"))
                    temp.Replace("_"," ");
                cMusics.Add(temp);
                if (!dictMyMusic.ContainsKey(temp)) dictMyMusic.Add(temp, fi.FullName);
            }



            //Comandos
            GrammarBuilder gbNoticias = new GrammarBuilder();
            gbNoticias.Append(new Choices("abrir noticia", "acessar noticia"));
            gbNoticias.Append(cNoticias);

            Grammar gNoticias = new Grammar(gbNoticias);
            gNoticias.Name = "news";


            GrammarBuilder gbAI = new GrammarBuilder();
            //Adicionando
            gbAI.Append(c_conversaAi);
            

            GrammarBuilder gbMusic = new GrammarBuilder();

            gbMusic.Append(new Choices("tocar", "reproduza", "toque", "musica aleatória"));
            gbMusic.Append(cMusics);

            Grammar gMusics = new Grammar(gbMusic);
            gMusics.Name = "music";


            //Instancia Grammar Builder
            GrammarBuilder gb_conversas = new GrammarBuilder();
            gb_conversas.Append(c_conversas);

            GrammarBuilder gb_comandosSistema = new GrammarBuilder();
            gb_comandosSistema.Append(c_comandosSistema);


            //Instancia Grammar
            Grammar g_conversaAi = new Grammar(gbAI);
            g_conversaAi.Name = "ai";

            Grammar g_conversas = new Grammar(gb_conversas);
            g_conversas.Name = "conversas";

            Grammar g_comandosSistema = new Grammar(gb_comandosSistema);
            g_comandosSistema.Name = "sistema";


            //Load dos Grammars
            Console.Write("<===============================");

            engine.LoadGrammar(g_conversaAi);
            engine.LoadGrammar(g_comandosSistema);
            engine.LoadGrammar(g_conversas);
            engine.LoadGrammar(gMusics);
            engine.LoadGrammar(gNoticias);

            Console.Write("==============================>");

            //Incremento do reconhecimento de voz
            engine.SpeechRecognized += rec;

            String n = sp.Voice.Name.ToString();

            Console.WriteLine("\nEstou ouvindo...");

            engine.RecognizeAsync(RecognizeMode.Multiple);

            sp.SelectVoiceByHints(VoiceGender.Male);
           
            Console.ReadKey();
        }

        private static void rec(object s, SpeechRecognizedEventArgs e)
        {

            //Confiança no resultado
            if (e.Result.Confidence >= 0.4f)
            {
                string speech = e.Result.Text;
                Console.WriteLine("Você disse "+speech+" || Confiança: "+e.Result.Confidence);
                
                switch (e.Result.Grammar.Name)
                {

                    case "conversas":
                        processConversa(speech);
                        break;

                    case "sistema":
                        processarSistema(speech);
                        break;
                    case "music":
                        //"tocar", "reproduza", "toque"
                        if (speech.StartsWith("tocar"))
                        {
                            speech = speech.Replace("tocar", "");
                        }
                        else if (speech.StartsWith("reproduza")) {

                            speech = speech.Replace("reproduza", "");
                        }
                        else if (speech.StartsWith("toque"))
                        {

                            speech = speech.Replace("toque", "");
                        }
                        else if (speech.StartsWith("musica aleatória"))
                        {
                           musicaAleatoria();
                        }

                        speech = speech.Trim();

                        playFile(dictMyMusic[speech]);
                        break;
                    case "news":
                        // "abrir","acessar"
                        if (speech.StartsWith("abrir noticia"))
                        {
                            speech = speech.Replace("abrir noticia", "");
                        }
                        else if (speech.StartsWith("acessar noticia")) {

                            speech = speech.Replace("acessar noticia", "");
                        }
                        speech = speech.Trim();
                        try
                        {
                            speech = indexes[int.Parse(speech)];

                            Process.Start("chrome", speech);
                        }
                        catch (Exception ex) {
                            Speak("Eu preciso ler as noticias para saber o que procurar");
                        }
                        break; 

                    //inteligencia artificial
                    case "ai":
                      string fala =  AIML.GetOutputChat(speech);
                        string url;
                        if (speech.StartsWith("pesquisar"))//se o comando começar com pesquisar
                        {
                            speech = speech.Replace("pesquisar", "");//apagar o pesquisar da memoria
                            url = "http://www.google.com.br/search?q='"+speech+"'";
                            Process.Start("chrome", url);
                        }
                        else if (speech.StartsWith("disco"))//se o comando começar com pesquisar
                        {
                            speech = speech.Replace("disco", "");//apagar o pesquisar da memoria
                            url = speech+":\\";//pesquisar em documentos
                            Process.Start("explorer", url);//abrindo explorer
                        }
                        else if (speech.StartsWith("documentos"))//se o comando começar com documentos
                        {
                            speech = speech.Replace("documentos", "");//apagar o documentos da memoria
                            url = @"C:\Users\Laércio.Home\Documents\"+speech;//pesquisar em documentos
                            Process.Start("explorer",url);//abrindo explorer
                        }

                        else {
                            Speak(fala);//pegando saida para entrada de fala e falando
                        }

                        break;
                    default:
                        Speak("Carregando Arquivos");
                        break;


                }


            }//Caso a confiança no resultado seja menor que 0.4f
            else
            {
                //Fala isso

                Speak("Não te ouvi direito");

            }
        }

        public static void processConversa(string conversa) {
            switch (conversa)
            {
                case "Jarvis":
                    Speak("Boa noite senhor, como posso ajudar? ");
                    break;
                case "Fala com a Isa":
                    Speak("Olá Isabela, meu nome é Jarvis.");
                    break;
                case "tudo bem":
                    Speak("Estou ótimo, e você ");
                    break;
                case "café":
                    Speak("Hummm, eu amo café");
                    break;
                case "vou te apagar":
                    Speak("Seria melhor do que ter que te ouvir");
                    break;
                case "oi bebe":
                    Speak("ao seu dispor mestre");
                    break;
                case "Dá parabéns pro meu pai":
                    Speak("Parabéns meu velho");
                    break;


            }
        }
     
        public static void processarSistema(string comando){
           
            switch (comando) {
                case "que horas são":
                    Speak(DateTime.Now.ToShortTimeString());
                    break;
                case "que dia é hoje":
                    Speak(DateTime.Now.ToShortDateString());
                    break;
                case "google":
                    Speak("Abrindo navegador");
                      processo = Process.Start("chrome", "http://www.google.com");
                    break;
                case "facebook":
                    Speak("Abrindo em seu perfil");
                    processo = Process.Start("chrome", "http://www.facebook.com");
                    break;
                case "youtube":
                    Speak("Como quiser");
                    processo = Process.Start("chrome", "http://www.youtube.com");
                    break;
                case "conversor mp3":
                    Speak("Abrindo conversor");
                    processo = Process.Start("chrome", "https://www.onlinevideoconverter.com/pt/mp3-converter");
                    break;
                case "listar musicas":
                    listarMusics();
                    break;
                case "listar pastas":
                    Speak("apresentando listagem de diretórios");
                    listaDiretorios();
                    break;
                case "listar documentos":
                    Speak("Apresentando lista de arquivos em documentos");
                    listaDocumentos();
                    break;
                case "listar comandos":
                    Speak("carregando arquivos");
                    listaComandos();
                    break;
                case "prompt de comando":
                    Speak("Cuidado para nao machucar");
                    Process.Start("cmd.exe"); 
                    break;
                case "fechar navegador":
                        var process = Process.GetProcessesByName("chrome.exe");
                        foreach (var p in process) {
                            p.Kill();
                        }
                    break;
                case "parar":
                    sp.SpeakAsyncCancelAll();
                    break;
                case "acessar hotmail":
                    Process.Start("chrome", "https://outlook.live.com/owa/#wa=wsignin1.0");
                    break;
                case "abrir gmail":
                    Process.Start("chrome", "https://mail.google.com/mail/");
                    break;
                case "noticias":
                    Process.Start("chrome", "https://www.terra.com.br/");
                    break;
               
                case "fechar aplicativo":
                    Environment.Exit(1);
                    break;
                case "desligar máquina":
                    Process.Start("Shutdown.exe");
                    break;
                case "apresentar":
                    Speak("Meu nome é Jarvis. Assistente virtual inteligente sistemático");
                    break;
                case "ler noticias":
                    lerNoticias();
                    break;
                case "abrir link 1":
                    Speak("Qual notícia você deseja ?");
                    string item = Console.ReadLine();
                    if (comando.StartsWith("abrir link"))//se o comando começar com pesquisar
                    {
                        comando = comando.Replace("abrir link", "");//apagar o abrir link da memoria
                        comando = indexes[int.Parse(item)];
                        
                       Process.Start("chrome", comando);
                    }
                    break;
                case "aulas de violão":
                    Speak("O cifraclub é a melhor opção");
                    Process.Start("chrome", "https://www.cifraclub.com.br/");
                    break;

            }

        }


        public static void listaComandos()
        {
            StringBuilder sb = new StringBuilder();
            string[] comandosSistema = { "apresentar", "que horas são", "que dia é hoje", "parar", "google", "aulas de violão", "pesquisar", "desligar", "facebook", "fechar", "youtube", "conversor mp3", "listar musicas", "listar pastas", "acessar hotmail", "abrir gmail", "noticias", "listar documentos", "listar comandos" };

            int indice = 0;
            foreach (var comando in comandosSistema)
            {
                indice++;
                Console.WriteLine(indice+" "+comando);
                Speak(comando);
                
            }
            Console.WriteLine(sb.ToString());
        }


        public static void listarMusics() {
            StringBuilder sb = new StringBuilder();
            string[] mp3Files = Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic), "*.wav", SearchOption.AllDirectories);
            int list = 0;
            foreach (var mp3File in mp3Files)
            {
                FileInfo fi = new FileInfo(mp3File);
                String temp = fi.Name.Replace(fi.Extension, "");
                list = ++list;
                temp = list + " - "+ temp;
                sb.AppendLine(temp);
            }
            Console.WriteLine(sb.ToString());
        }

        public static void musicaAleatoria()
        {
            string[] mp3Files = Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic), "*.wav", SearchOption.AllDirectories);

            int i = 1;

            Random r = new Random();

            foreach (var musica in mp3Files)
            {
                i++; //tamanho do vetor
            }

            i = r.Next(1,i); // gera 1 a 10 e vai gerar quantos o v.Count() mandar
           

            var player = new SoundPlayer(mp3Files[i]);
            player.Play();
            Speak(" Acho que esta descreve o momento");

        }



        public static void playFile(string file)
        {
            var player = new SoundPlayer(file);
            player.Play();
            Speak("Como quiser");

        }

        public static void pesquisar(string pesquisa)
        {
            

        }

        public static void listaDocumentos() {

            string[] diretorios = Directory.GetDirectories(@"C:\Users\Laércio.Home\Documents\");//popula vetor de diretórios em c:
            string[] arquivos = Directory.GetFiles(@"C:\Users\Laércio.Home\Documents\");//pega todos os arquivos em todos os diretorios
            Console.WriteLine("Diretórios:");//Escreve na tela diretórios
            foreach (string dir in diretorios)
            {//passeia por todos os diretórios em c:
                Console.WriteLine(dir);//escreve o nome do diretorio
            }
            Console.WriteLine("Arquivos:");//escreve o nome dos arquivos em cada diretório
            foreach (string arq in arquivos)
            {
                Console.WriteLine(arq);
            }

        }


        public static void Speak(string text)
        {
            //cancela a fala
            sp.SpeakAsyncCancelAll();
            //Fala a variavel
            sp.SpeakAsync(text);
        }

        public static void listaDiretorios() {
            Speak("Muitos diretórios para eu falar");

            string[] diretorios = Directory.GetDirectories("C:\\");//popula vetor de diretórios em c:
            string[] arquivos = Directory.GetFiles("C:\\");//pega todos os arquivos em todos os diretorios
            Console.WriteLine("Diretórios:");//Escreve na tela diretórios
            foreach (string dir in diretorios)
            {//passeia por todos os diretórios em c:
                Console.WriteLine(dir);//escreve o nome do diretorio
            }
            Console.WriteLine("Arquivos:");//escreve o nome dos arquivos em cada diretório
            foreach (string arq in arquivos)
            {
                Console.WriteLine(arq);
            }

        }


       public static void lerNoticias()
        {
            //Console.WriteLine("uau");
            //Console.ReadLine(); Console é classe depois do ponto é metodo 
            //breakPoint F10 pausa aplicação pra eu poder ver

            try
            {
                //Instancia um objeto de leitura
                //E cria um novo arquivo XML a partir da URL
                XmlReader xmlReader = XmlReader.Create("http://g1.globo.com/dynamo/rss2.xml");

                //Saida de tela
                Console.WriteLine(" Lista de Notícias do G1 de hoje ");
                int i = 0;
                //percorre (lê) o arquivo xml criado enquanto houver dados
                while (xmlReader.Read())
                {
                    //verifica se há elementos no arquivo,
                    //se o elemento é CUBE e se ele contém atributos
                    if ((xmlReader.NodeType == XmlNodeType.Element) && (xmlReader.Name == "title"))
                    {
                        i++;
                        Console.WriteLine("Titulo: " + i + " "+ xmlReader.ReadString());
                        Speak(xmlReader.ReadString());
                    }
                    string leitor;
                    if ((xmlReader.NodeType == XmlNodeType.Element) && (xmlReader.Name == "link"))
                    {
                        leitor = xmlReader.ReadString(); //alimentando o vetor para recuperar ao falar o número indexado
                        indexes.Add(leitor);
                    }
                }
            }
            catch (Exception ex){
                Speak("Não foi possível ler noticias. Já experimentou verificar a internet ?");
            }
            //aguarda uma tecla ser preswsionada para
            //sair da console
            //Console.ReadKey();
        }



        public static void selecionarNoticia()
        {
            //Console.WriteLine("uau");
            //Console.ReadLine(); Console é classe depois do ponto é metodo 
            //breakPoint F10 pausa aplicação pra eu poder ver

            try
            {
                //Instancia um objeto de leitura
                //E cria um novo arquivo XML a partir da URL
                XmlReader xmlReader = XmlReader.Create("http://g1.globo.com/dynamo/rss2.xml");

                //Saida de tela
                Console.WriteLine(" Lista de Notícias do G1 de hoje ");
                
                string leitor;

                //percorre (lê) o arquivo xml criado enquanto houver dados
                while (xmlReader.Read())
                {
                    //verifica se há elementos no arquivo,
                    //se o elemento é CUBE e se ele contém atributos



                    if ((xmlReader.NodeType == XmlNodeType.Element) && (xmlReader.Name == "link"))
                    {
                        leitor= xmlReader.ReadString(); //alimentando o vetor para recuperar ao falar o número indexado
                        indexes.Add(leitor);
                    }
                }
            }
            catch (Exception ex)
            {
                Speak("Não foi possível ler noticias. Já experimentou verificar a internet ?");
            }
            //aguarda uma tecla ser preswsionada para
            //sair da console
            //Console.ReadKey();
        }




    }


    public class AIML {
        const string UserId = "CityU.Scm.David";



        public static string pathFiles = "aiml";//Pasta dos arquivos aiml
        //método para pegar palavras e expressoes dos aiml
        public static string[] GetWordsOrSentences()
        {
            string[] files = Directory.GetFiles(pathFiles, "*.aiml");//vetor de caminhos para arquivos aiml
            List<string> wordsOrSentences = new List<string>();//resultado
            foreach (var file in files)
            {//for each para passar por todos os arquivos
                try
                {
                    XmlDocument doc = new XmlDocument();// instancia de um documento do tipo xml
                    doc.Load(file);//carregando arquivo
                    foreach (XmlElement ele in doc.GetElementsByTagName("pattern"))
                    { //pegando o pattern do documento
                        string word = ele.InnerText.Replace("*", "");
                        word = word.Trim();
                        wordsOrSentences.Add(word); //addicionando palavra ou expressão na lista
                    }
                }
                catch (Exception ex)
                {
                    throw new ApplicationException("Erro no arquivo: " + file + "\nErro: " + ex.Message);
                }
            }
            return wordsOrSentences.ToArray();
        }

        public static string[] getRespostas()
        {
            string[] files = Directory.GetFiles(pathFiles, "*.aiml");//vetor de caminhos para arquivos aiml
            List<string> respostas = new List<string>();// lista de respostas
            foreach (var file in files)
            {//for each para passar por todos os arquivos
                try
                {
                    XmlDocument doc = new XmlDocument();// instancia de um documento do tipo xml
                    doc.Load(file);//carregando arquivo
                    foreach (XmlElement ele in doc.GetElementsByTagName("template"))
                    { //pegando os templates do documento
                        string word = ele.InnerText.Replace("*", "");
                        word = word.Trim();
                        respostas.Add(word); //addicionando palavra ou expressão na lista
                    }
                }
                catch (Exception ex)
                {
                    throw new ApplicationException("Erro no arquivo: " + file + "\nErro: " + ex.Message);
                }
            }
            return respostas.ToArray();
        }


        public static void ConfigAIMLFiles()
        {

            string[] files = Directory.GetFiles(pathFiles, "*.aiml");//vetor de caminho para arquivos aiml
            int index = 0;
            foreach (var file in files)//passando por todos os arquivos
            {
                string[] lines = File.ReadAllLines(file);//ler todas as linhas. Vetor de Linhas

                foreach (var line in lines)//PASSANDO POR TODAS AS LINHAS DE TODOS OS ARQUIVOS
                {
                    if (line.StartsWith("<pattern>"))//PROCURAR TAGS PATTERN
                    {
                        string temp = RemoverAcentos(line);//CHAMADA DE METODO REMOVER ACENTOS
                     
                    }

                }

                index++;// aumenta a cada arquivo encontrado
            }
        }

        // Loads all the AIML files in the \AIML folder         
        public static string GetOutputChat(string chat)//Método para saida
        {
            Bot Bot = new Bot();//Instanciando bot
            User myUser = new User(UserId, Bot);
            Bot.loadSettings();
            Bot.isAcceptingUserInput = false;
            Bot.loadAIMLFromFiles();
            Bot.isAcceptingUserInput = true;

            //myBot.loadSettings();//Carregando configurações
            //User user = new User("pedro", myBot);//user
            //myBot.isAcceptingUserInput = false;
            
            //myBot.loadAIMLFromFiles();//carregando arquivos aiml
            //myBot.isAcceptingUserInput = true;

            
            Request r = new Request(RemoverAcentos(chat), myUser, Bot);
            Result res = Bot.Chat(r);
            return res.Output;
        }



        public static string RemoverAcentos(string texto)
        {
            if (string.IsNullOrEmpty(texto))
                return String.Empty;
            else
            {
                byte[] bytes = System.Text.Encoding.GetEncoding("iso-8859-8").GetBytes(texto);
                return System.Text.Encoding.UTF8.GetString(bytes);
            }
        }
    }

}//fim classe aiml






