using System;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;

namespace DeSerializtion
{
    class Program
    {
        const int NodesCount = 100000;

        static void Main(string[] args)
        {
            var tmpFileName = Path.GetTempFileName();
            var sourceList = CreateList();
            using (var file = new FileStream(tmpFileName, FileMode.Truncate, FileAccess.Write))
            {
                sourceList.Serialize(file);
            }

            Console.WriteLine($"Serialized list of {NodesCount} items to {tmpFileName}");

            var deserializedList = new ListRand();
            using (var file = new FileStream(tmpFileName, FileMode.Open, FileAccess.Read))
            {
                deserializedList.Deserialize(file);
            }

            Console.WriteLine($"Deserialized list of {deserializedList.Count} items from {tmpFileName}");

            if (Check(sourceList, deserializedList))
                Console.WriteLine("Check done. List are equals");
            else
                Console.WriteLine("Check done. Something is bad :(");

            Console.ReadKey();
        }

        static bool Check(ListRand listOne, ListRand listTwo)
        {
            var one = listOne.Head;
            var two = listTwo.Head;
            var isOk = true;
            while (one != null)
            {
                if (one.Data != two.Data || one.Rand?.Data != two.Rand?.Data)
                {
                    isOk = false;
                    break;
                }
                one = one.Next;
                two = two.Next;
            }

            return isOk;
        }

        static ListRand CreateList()
        {
            var random = new Random(100500);

            var data = Enumerable.Range(0, NodesCount).Select(x => new ListNode()).ToList();
            for (var idx = 0; idx < NodesCount; idx++)
            {
                var randomNumber = random.Next(10);

                data[idx].Next = idx < NodesCount - 1 ? data[idx + 1] : null;
                data[idx].Prev = idx > 0 ? data[idx - 1] : null;
                data[idx].Data = randomNumber == 1 
                    ? null 
                    : (randomNumber == 2 ? string.Empty :  idx.ToString()); //Примерно в 10% случаях будем иметь null строку, и в 10% - пустую строку
                data[idx].Rand = randomNumber == 2 
                    ? null //Примерно в 10% случаях будем иметь Null ссылку
                    : data[random.Next(NodesCount)]; 
            }
            var result = new ListRand
            {
                Count = NodesCount,
                Head = data[0],
                Tail = data[NodesCount - 1]
            };
            return result;
        }
    }
}
