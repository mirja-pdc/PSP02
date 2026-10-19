using System;
using System.Threading;

class Caballos
{
    static int caballos = 4;
    static int longitud = 200;
    static int maxAvance = longitud / 10; // El 10% de la longitud
    static int[] avances = new int[caballos];
    static Random rnd = new Random();
    static object bloquear = new object();
    // Variable global para saber si la carrera ha terminado
    static bool carreraTerminada = false;
    // Variable global para guardar el caballo ganador
    // Se inicializa a -1 para no confundir su valor inicial
    // con el índice de ningún caballo
    static int caballoGanador = -1;

    static void Main(string[] args)
    {
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

    static void Correr(object id) // El parámetro id es de tipo object
    {
        // Se usa (int)id para convertir ("casting")
        // el id de un object a un int
        int idCaballo = (int)id;
 
        // Mientras la carrera NO haya terminado y el caballo no llegue a la meta
        while (!carreraTerminada && avances[idCaballo] < longitud)
        {
            // Se crea un valor aleatorio por cada caballo 
            // y se avanza dicho valor en la longitud del circuito.
            // El valor del random en cada tirada, gracias a maxAvance,
            // no puede ser superior al 10% de la longitud.
            // El método Next genera un número aleatorio desde el primer número 
            // incluido hasta el segundo excluido, por lo que 
            // se le pone +1 a maxAvance para que coja también el valor máximo.
            int avance = rnd.Next(1, maxAvance + 1); // Un número aleatorio entre 1 y el máximo permitido
            
            // Solo un caballo puede cruzar la meta 
            // en cada jugada. Se controla con un lock.
            lock (bloquear)
            {
                if (!carreraTerminada) // Doble comprobación de seguridad dentro del lock,
                // por si otro caballo ha cruzado la meta mientras este caballo estaba esperando su turno
                {
                    avances[idCaballo] += avance;
                    Console.WriteLine("Soy el caballo {0} y el valor de avance hasta el momento es de {1} m", idCaballo + 1, avances[idCaballo]);

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
        }

    }

}
