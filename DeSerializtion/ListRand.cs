using System.IO;
using System.Text;

namespace DeSerializtion
{
    /// <summary>
    /// Основная сложность, на мой взгляд - связанный список, к которому нет возможности обращаться по индексу.
    /// Самое простое решение - загнать элементы связанного списка в List или массив, и далее серилизация / десерилизация - уже дело техники.
    /// 
    /// Но мы не ищем легких путей и реализуем алгоритм без дополнительного списка / массива.
    /// Проиграем по процессору, но выиграем по памяти. 
    /// </summary>
    class ListRand
    {
        public ListNode Head;
        public ListNode Tail;
        public int Count;

        public void Serialize(FileStream s)
        {
            using (var binaryWriter = new BinaryWriter(s, Encoding.UTF8))
            {
                binaryWriter.Write(Count); //Запишем в файл кол-во нод
                binaryWriter.BaseStream.Seek(sizeof(int) * Count, SeekOrigin.Current); //Оставим место под хранение индексов Rand

                var currentNodeIndex = 0;
                var current = Head;
                while (current != null)
                {
                    SerializeListNode(current, currentNodeIndex, binaryWriter);
                    current = current.Next;
                    currentNodeIndex++;
                }
            }
        }

        public void Deserialize(FileStream s)
        {
            using (var binaryReader = new BinaryReader(s, Encoding.UTF8))
            {
                Count = binaryReader.ReadInt32(); //Считаем общее количество
                binaryReader.BaseStream.Seek(sizeof(int) * Count, SeekOrigin.Current); //Пропустим место под хранение индексов Rand

                ListNode currentNode;

                //Сначала восстановим все ноды
                for (var currentNodeIndex = 0; currentNodeIndex < Count; currentNodeIndex++)
                {
                    currentNode = DeserializeListNode(binaryReader); //Читаем данные ноды
                    if (currentNodeIndex == 0)
                    {
                        Tail = Head = currentNode; //Если это начало - запомним и в голову и в хвост :)
                    }
                    else
                    {
                        //Добавляем новую ноду в хвост
                        Tail.Next = currentNode;
                        currentNode.Prev = Tail;
                        Tail = currentNode;

                        if (currentNodeIndex == 1) //Если это первая нода - ностроим голову
                            Head.Next = Tail;
                    }
                }

                //А теперь восстановим значения Rand полей
                binaryReader.BaseStream.Seek(sizeof(int), SeekOrigin.Begin); //Переместимся в секцию индексов Rand
                currentNode = Head;
                for (var currentNodeIndex = 0; currentNodeIndex < Count; currentNodeIndex++)
                {
                    var randNodeIndex = binaryReader.ReadInt32(); //Прочитаем смещение Rand для текущей ноды
                    currentNode.Rand = GetNodeByIndex(randNodeIndex); //Найдем эту ноду и запомним ссылку на нее
                    currentNode = currentNode.Next; //Переместимся к следующей ноде
                }
            }
        }

        /// <summary>
        /// Находит ноду по ее индексу относительно головы списка
        /// </summary>
        /// <param name="index">Индекс ноды</param>
        /// <returns>Нода с указанным индексом, или null, если индекс меньше нуля</returns>
        private ListNode GetNodeByIndex(int index)
        {
            if (index == -1)
                return null;

            var current = Head;
            var currentIndex = 0;
            while (current != null)
            {
                if (currentIndex == index)
                    return current;
                currentIndex++;
                current = current.Next;
            }

            return null;
        }

        /// <summary>
        /// Десерилизует ноду из потока, используя <see cref="BinaryReader"/> 
        /// <remarks>На данном этапе мы не восстанавливаем Rand поле</remarks>
        /// </summary>
        /// <param name="br">Бинарный читатель :) владеющий потоком</param>
        /// <returns>Нода без значения Rand поля</returns>
        private static ListNode DeserializeListNode(BinaryReader br)
        {
            var node = new ListNode();
            var hasData = br.ReadBoolean();
            if (hasData)
                node.Data = br.ReadString();
            
            return node;
        }

        /// <summary>
        /// Серилизует ноду в поток, используя <see cref="BinaryWriter"/>
        /// </summary>
        /// <param name="node">Нода</param>
        /// <param name="currentNodeIndex">Индекс ноды относительно головы списка</param>
        /// <param name="bw">Бинарный писатель :) владеющий потоком</param>
        private static void SerializeListNode(ListNode node, int currentNodeIndex, BinaryWriter bw)
        {
            var currentPosition = bw.BaseStream.Position; //Запомним текущую позицию в потоке
            var randNodeIndex = GetNodeIndex(node.Rand);  //Вычислим индекс Rand ноды

            bw.BaseStream.Seek(currentNodeIndex * sizeof(int) + sizeof(int), SeekOrigin.Begin); //Переместимся в секцию индексов Rand
            bw.Write(randNodeIndex); //Запишем смещение Rand для текущей ноды

            bw.BaseStream.Seek(currentPosition, SeekOrigin.Begin);

            var hasData = node.Data != null;
            bw.Write(hasData); //Запишем признак того, что строка данных не равна null
            if (hasData)
                bw.Write(node.Data); //Если строка данных не равна null, запишем ее
        }

        /// <summary>
        /// Вычисляет позицию ноды относительно головы списка
        /// </summary>
        /// <param name="node">Нода, для которой рассчитывается позиция</param>
        /// <returns>
        /// Индекс ноды относительно головы списка или -1, если нода равна null
        /// </returns>
        private static int GetNodeIndex(ListNode node)
        {
            if (node == null)
                return -1;

            var idx = 0;
            while (node.Prev != null)
            {
                idx++;
                node = node.Prev;
            }

            return idx;
        }
    }
}
