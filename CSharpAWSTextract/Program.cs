using Amazon.Textract;
using Amazon.Textract.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpAWSTextract
{
    class Program
    {
        static void Main(string[] args)
        {
                        
            const string LocalEmploymentFile = "TextractImages/invoice1.png";

            TextExtractDemo.Get_kv_map(LocalEmploymentFile);

        }

    }
    public class TextExtractDemo
    {
        public static void Get_kv_map(string LocalEmploymentFile)
        {
            var readFile = File.ReadAllBytes(LocalEmploymentFile);

            MemoryStream stream = new MemoryStream(readFile);
            AmazonTextractClient abcdclient = new AmazonTextractClient();

            AnalyzeDocumentRequest analyzeDocumentRequest = new AnalyzeDocumentRequest
            {
                Document = new Document
                {
                    Bytes = stream

                },
                FeatureTypes = new List<string>
                {
                    FeatureType.FORMS
                }


            };
            var analyzeDocumentResponse = abcdclient.AnalyzeDocument(analyzeDocumentRequest);

            //Get the text blocks
            List<Block> blocks = analyzeDocumentResponse.Blocks;

            //get key and value maps
            List<Block> key_map = new List<Block>();
            List<Block> value_map = new List<Block>();
            List<Block> block_map = new List<Block>();

            foreach (Block block in blocks)
            {
                var block_id = block.Id;
                block_map.Add(block);
                if (block.BlockType == BlockType.KEY_VALUE_SET)
                {
                    if (block.EntityTypes.Contains("KEY"))
                    {
                        key_map.Add(block);
                    }
                    else
                    {
                        value_map.Add(block);
                    }

                }

            }

            //Get Key Value relationship
            var getKeyValueRelationship = Get_kv_relationship(key_map, value_map, block_map);

            foreach (KeyValuePair<string, string> kvp in getKeyValueRelationship)
            {
                Console.WriteLine(" {0} : {1}", kvp.Key, kvp.Value);
            }

        }
        public static Dictionary<string, string> Get_kv_relationship(List<Block> key_map, List<Block> value_map, List<Block> block_map)
        {
            List<string> kvs1 = new List<string>();
            Dictionary<string, string> kvs = new Dictionary<string, string>();
            Block value_block = new Block();
            string key, val = string.Empty;
            foreach (var block in key_map)
            {
                value_block = Find_value_block(block, value_map);
                key = Get_text(block, block_map);
                val = Get_text(value_block, block_map);
                kvs.Add(key, val);
            }

            return kvs;

        }

        public static Block Find_value_block(Block block, List<Block> value_map)
        {
            Block value_block = new Block();
            foreach (var relationship in block.Relationships)
            {
                if (relationship.Type == "VALUE")
                {
                    foreach (var value_id in relationship.Ids)
                    {
                        value_block = value_map.First(x => x.Id == value_id);
                    }

                }

            }
            return value_block;

        }

        public static string Get_text(Block result, List<Block> block_map)
        {
            string text = string.Empty;
            Block word = new Block();

            if (result.Relationships.Count > 0)
            {
                foreach (var relationship in result.Relationships)
                {
                    if (relationship.Type == "CHILD")
                    {
                        foreach (var child_id in relationship.Ids)
                        {
                            word = block_map.First(x => x.Id == child_id);
                            if (word.BlockType == "WORD")
                            {
                                text += word.Text + " ";
                            }
                            if (word.BlockType == "SELECTION_ELEMENT")
                            {
                                if (word.SelectionStatus == "SELECTED")
                                {
                                    text += "X ";
                                }

                            }
                        }
                    }
                }
            }
            return text;

        }
    }
}
