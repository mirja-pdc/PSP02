using System;
using System.Threading;

class Caballos
{
    // Hay 4 hilos o caballos que corren en paralelo, cada uno con su propio avance
    static int caballos = 4;
    // La longitud de la carrera es de 200 metros
    static int longitud = 200;
    // El avance máximo de un caballo en una tirada es del 10% de la longitud total
    static int maxAvance = longitud / 10; 
    // El array avances guarda el avance de cada caballo, que se inicializa a 0
    static int[] avances = new int[caballos];
    // Objeto Random para generar números aleatorios
    static Random rnd = new Random();
    // Objeto para block en la sección de avanzar del método Correr, 
    // para que solo un caballo pueda avanzar a la vez
    static object bloquear = new object();
    // Variable global para saber si la carrera ha terminado
    static bool carreraTerminada = false;
    // Variable global para guardar el caballo ganador
    // Se inicializa a -1 para no confundir su valor inicial
    // con el índice de ningún caballo
    static int caballoGanador = -1;

    static void Main(string[] args)
    {
        int opcion = 0;

        // Bucle para mostrar el menú de la consola repetidamente hasta elegir Salir (3)
        while (opcion != 3)
        {
            Console.WriteLine("\n==================================");
            Console.WriteLine("    MENÚ - CARRERA DE CABALLOS    ");
            Console.WriteLine("==================================");
            Console.WriteLine("1. Empezar juego");
            Console.WriteLine("2. Reiniciar juego");
            Console.WriteLine("3. Salir del juego");
            Console.Write("Elige una opción: ");

            // Se lee la opción elegida por el usuario
            if (int.TryParse(Console.ReadLine(), out opcion))
            {
                switch (opcion)
                {
                    case 1:
                        // Se llama al método para iniciar la carrera
                        IniciarCarrera();
                        break;
                    case 2:
                        // Se reinicia la carrera
                        ReiniciarCarrera();
                        break;
                    case 3:
                        Console.WriteLine("Saliendo del juego...");
                        break;
                    default:
                        Console.WriteLine("Opción no válida. Por favor, elige 1, 2 o 3.");
                        break;
                }
            }
            else
            {
                Console.WriteLine("Entrada no válida. Por favor, introduce un número.");
            }
        }
    }

    static void IniciarCarrera()
    {
        // Se reinician las variables globales por si venimos de otra partida
        // (los hilos mueren con la partida al acabar de ejecutarse el método Correr,
        // pero las variables globales siguen con los valores de la última partida)
        carreraTerminada = false;
        caballoGanador = -1;
        for (int i = 0; i < caballos; i++)
        {
            avances[i] = 0;
        }

        // Se define un array de hilos para los 4 caballos
        Thread[] caballo = new Thread[caballos];

        // Se define un array con las prioridades de los 4 caballos
        ThreadPriority[] prioridades = { 
            ThreadPriority.Highest, 
            ThreadPriority.Normal, 
            ThreadPriority.Lowest, 
            ThreadPriority.AboveNormal 
        };

        // Primero Start de todos los hilos y luego Join de todos,  
        // para ejecución paralela (simultánea) de los hilos
        for (int i = 0; i < caballos; i++)
        {
            caballo[i] = new Thread(Correr);

            // Se asigna una prioridad a cada caballo según el array de prioridades
            caballo[i].Priority = prioridades[i];

            // Start toma como parámetro un object, el índice del caballo,
            // que recibe como parámetro el método Correr, para que cada hilo/caballo 
            // pueda saber cuando está corriendo qué caballo es y guardar sus avances
            caballo[i].Start(i);
        }

        // El Main espera a que dejen de correr los 4 caballos 
        // para imprimir el ganador y el podio, pero los
        // caballos no se esperan entre sí (corren independientemente
        // gracias a que los 4 tienen Start)
        for (int i = 0; i < caballos; i++)
        {
            caballo[i].Join();
        }

        // Cuando acaba el juego, se muestran el caballo ganador
        // y la posición de cada caballo en el carril/podium 
        // Se suma 1 a caballoGanador para que no se muestre el índice 0
        Console.WriteLine("Ganador: Caballo {0}", caballoGanador + 1);
        for (int i = 0; i < caballos; i++)
        {
            Console.WriteLine("Caballo {0}: {1}", i + 1, avances[i]);
        } 
    }

    static void ReiniciarCarrera()
    {
        // Se reinician las variables globales para poder volver a jugar
        carreraTerminada = false;
        caballoGanador = -1;

        // Se reinicia el array de avances a 0 para todos los caballos
        for (int i = 0; i < caballos; i++)
        {
            avances[i] = 0;
        }

        Console.WriteLine("Juego reiniciado. Volviendo al menú principal...");
    }

    static void Correr(object id) // El parámetro id es de tipo object
    {
        // Se usa (int)id para convertir ("casting")
        // el id de un object a un int
        int idCaballo = (int)id;
 
        // Mientras la carrera NO haya terminado y el caballo no llegue a la meta
        while (!carreraTerminada && avances[idCaballo] < longitud)
        {
            // Se crea un valor aleatorio por cada caballo para luego avanzar 
            // dicho valor en la longitud del circuito (dentro del lock).
            // El valor del random en cada tirada, gracias a maxAvance,
            // no puede ser superior al 10% de la longitud.
            // El método Next genera un número aleatorio desde el primer número 
            // incluido hasta el segundo excluido, por lo que 
            // se le pone +1 a maxAvance para que coja también el valor máximo.
            int avance = rnd.Next(1, maxAvance + 1); // Un número aleatorio entre 1 y el máximo permitido
            
            // Solo un caballo puede cruzar la meta 
            // en cada jugada. Se controla con un lock
            // sobre cada avance.
            lock (bloquear)
            {
                if (!carreraTerminada) // Doble comprobación de seguridad dentro del lock,
                // por si otro caballo ha cruzado la meta mientras este caballo estaba esperando su turno
                {
                    avances[idCaballo] += avance;
                    Console.WriteLine("Soy el caballo{0} y el valor de avance hasta el momento es de {1} m", idCaballo + 1, avances[idCaballo]);

                    // Si este caballo alcanza o supera la longitud total,
                    // se da por terminada la carrera
                    if (avances[idCaballo] >= longitud)
                    {
                        carreraTerminada = true; // Se acaba la carrera para todos
                        // Se guarda el caballo ganador en la variable global
                        caballoGanador = idCaballo;
                    }
                }

            }

            // Pausa breve para que la consola imprima los avances de forma legible
            Thread.Sleep(500); // 1 segundo de pausa entre cada tirada
        }
    }
}
